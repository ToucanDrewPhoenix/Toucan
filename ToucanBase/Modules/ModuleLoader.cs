using System.IO;

namespace Toucan.Modules
{

internal class ModuleLoader
{
    #region Public

    public static string LoadModule( string moduleName )
    {
        using ( Stream stream =
               typeof( ModuleLoader ).Assembly.GetManifestResourceStream( $"Toucan.Modules.{moduleName}.Toucan" ) )
        {
            StreamReader reader = new StreamReader( stream );

            return reader.ReadToEnd();
        }
    }

    #endregion
}

}
