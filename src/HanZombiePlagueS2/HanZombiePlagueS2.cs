using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.Services;

namespace HanZombiePlagueS2;

[PluginMetadata(
    Id = "HanZombiePlagueS2",
    Version = "1.9.1",
    Name = "CS2 僵尸瘟疫 for Sw2/CS2 ZombiePlague for Sw2",
    Author = "H-AN",
    Description = "CS2 僵尸瘟疫 SW2版本 CS2 ZombiePlague for SW2.")]

public partial class HanZombiePlagueS2(ISwiftlyCore core) : BasePlugin(core)
{

    private ServiceProvider? ServiceProvider { get; set; }
    private static readonly HanZombiePlagueAPI _apiInstance = new();
    private HZPMainCFG _HZPMainCFG = null!;
    private HZPGlobals _Globals = null!;
    private HZPEvents _Events = null!;
    private HZPCommands _Commands = null!;
    private HZPServices _Services = null!;
    private Guid _messageHook;

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
        interfaceManager.AddSharedInterface<IHanZombiePlagueAPI, HanZombiePlagueAPI>("HanZombiePlague", _apiInstance);
    }
    public override void Load(bool hotReload)
    {
        if (_messageHook == Guid.Empty)
            _messageHook = Core.NetMessage.HookServerMessage<CMsgPlaceDecalEvent>(_ => HookResult.Stop);

        Core.Configuration.InitializeJsonWithModel<HZPMainCFG>("HZPMainCFG.jsonc", "HZPMainCFG").Configure(builder =>
        {
            builder.AddJsonFile("HZPMainCFG.jsonc", false, true);
        });
        Core.Configuration.InitializeJsonWithModel<HZPVoxCFG>("HZPVoxCFG.jsonc", "HZPVoxCFG").Configure(builder =>
        {
            builder.AddJsonFile("HZPVoxCFG.jsonc", false, true);
        });
        Core.Configuration.InitializeJsonWithModel<HZPZombieClassCFG>("HZPZombieClassCFG.jsonc", "HZPZombieClassCFG").Configure(builder =>
        {
            builder.AddJsonFile("HZPZombieClassCFG.jsonc", false, true);
        });
        Core.Configuration.InitializeJsonWithModel<HZPSpecialClassCFG>("HZPSpecialClassCFG.jsonc", "HZPSpecialClassCFG").Configure(builder =>
        {
            builder.AddJsonFile("HZPSpecialClassCFG.jsonc", false, true);
        });

        
        var collection = new ServiceCollection();
        collection.AddSwiftly(Core);
        collection.AddSingleton<IHanZombiePlagueAPI>(_apiInstance);
        collection.AddSingleton(_apiInstance);

        collection
            .AddOptionsWithValidateOnStart<HZPMainCFG>()
            .BindConfiguration("HZPMainCFG");

        collection
            .AddOptionsWithValidateOnStart<HZPVoxCFG>()
            .BindConfiguration("HZPVoxCFG");

        collection
            .AddOptionsWithValidateOnStart<HZPZombieClassCFG>()
            .BindConfiguration("HZPZombieClassCFG");

        collection
            .AddOptionsWithValidateOnStart<HZPSpecialClassCFG>()
            .BindConfiguration("HZPSpecialClassCFG");

        collection.AddSingleton<HZPGlobals>();
        collection.AddSingleton<HZPEvents>();
        collection.AddSingleton<HZPHelpers>();
        collection.AddSingleton<HZPServices>();
        collection.AddSingleton<HZPCommands>();
        collection.AddSingleton<PlayerZombieState>();
        collection.AddSingleton<HZPMenuHelper>();
        collection.AddSingleton<HZPZombieClassMenu>();
        collection.AddSingleton<HZPAdminItemMenu>();
        collection.AddSingleton<HZPGameMode>();


        ServiceProvider = collection.BuildServiceProvider();

        _apiInstance.Initialize(
            Core,
            ServiceProvider.GetRequiredService<ILogger<HanZombiePlagueAPI>>(),
            ServiceProvider.GetRequiredService<HZPGlobals>(),
            ServiceProvider.GetRequiredService<HZPHelpers>(),
            ServiceProvider.GetRequiredService<HZPServices>(),
            ServiceProvider.GetRequiredService<IOptionsMonitor<HZPMainCFG>>(),
            ServiceProvider.GetRequiredService<PlayerZombieState>(),
            ServiceProvider.GetRequiredService<IOptionsMonitor<HZPZombieClassCFG>>(),
            ServiceProvider.GetRequiredService<IOptionsMonitor<HZPSpecialClassCFG>>(),
            ServiceProvider.GetRequiredService<HZPGameMode>()
        );

        _Globals = ServiceProvider.GetRequiredService<HZPGlobals>();
        _Events = ServiceProvider.GetRequiredService<HZPEvents>();
        _Commands = ServiceProvider.GetRequiredService<HZPCommands>();
        _Services = ServiceProvider.GetRequiredService<HZPServices>();

        var ZriotCFGMonitor = ServiceProvider.GetRequiredService<IOptionsMonitor<HZPMainCFG>>();
        _HZPMainCFG = ZriotCFGMonitor.CurrentValue;
        ZriotCFGMonitor.OnChange(newConfig =>
        {
            _HZPMainCFG = newConfig;
            Core.Logger.LogInformation(Core.Localizer["ServerInfoHotReload"]); 
        });

        _Events.HookEvents();
        _Events.HookZombieSoundEvents();
        _Commands.Command();
        _Commands.MenuCommands();
    }


    public override void Unload()
    {
        if (_messageHook != Guid.Empty)
        {
            Core.NetMessage.Unhook(_messageHook);
            _messageHook = Guid.Empty;
        }

        _Services?.ResetPluginRuntimeState();
        _apiInstance!.Dispose();
        ServiceProvider!.Dispose();
    }

    
}
