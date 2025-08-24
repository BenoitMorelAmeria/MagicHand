using System;

namespace Ameria.Maverick
{
    public class PackageConstants
    {
        public const string PACKAGE_JSON_FILE       = "mpackage.json";
        public const string PACKAGE_ICON_NAME       = "icon.png";

        public const string PACKAGE_SCHEMA_INSTALL  = "maverick/package/install";
        public const string PACKAGE_SCHEMA_RUNTIME  = "maverick/package/runtime";
        public const string PACKAGE_SCHEMA_APP      = "maverick/package/app";
    }

    public enum StartType
    {
        None,
        User,
        WinLogon,
        UserAndWinLogon,
        Service
    }

    public enum RestartBehaviour
    {
        Restart
    }

}