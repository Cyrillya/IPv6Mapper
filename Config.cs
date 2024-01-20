using System.ComponentModel;
using IPv6Mapper.ConfigElements;
using MachineTranslate.ConfigElements;
using Terraria.ModLoader.Config;

namespace IPv6Mapper;

public class Config : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [CustomModConfigItem(typeof(OpenFile))] public object OpenFile;
    [CustomModConfigItem(typeof(OpenNetworkInfo))] public object OpenNetworkInfo;
    [CustomModConfigItem(typeof(OpenNetworkControl))] public object OpenNetworkControl;
    [CustomModConfigItem(typeof(OpenFirewall))] public object OpenFirewall;
    [CustomModConfigItem(typeof(Input))] public string CustomIPv6Address;
    [DefaultValue("40800")] [CustomModConfigItem(typeof(Input))] public string CustomMappedRemotePort;
    [DefaultValue("26000")] [CustomModConfigItem(typeof(Input))] public string CustomMappedLocalPort;
    [CustomModConfigItem(typeof(IPv6Test1))] public object IPv6Test1;
    [CustomModConfigItem(typeof(IPv6Test2))] public object IPv6Test2;
    [CustomModConfigItem(typeof(IPv6Test3))] public object IPv6Test3;
    [CustomModConfigItem(typeof(IPv6Tutorial))] public object IPv6Tutorial;
}
