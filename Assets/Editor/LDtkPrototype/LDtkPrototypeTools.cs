using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    // LDtkToUnity 6.12.4 keeps the bundled exporter installer internal.
    // Run once per checkout; generated executables and local paths stay in Library.
    public static class LDtkPrototypeTools
    {
        [MenuItem("SacredDrum/LDtk/Prepare Local Tileset Exporter")]
        public static void PrepareExporter()
        {
            var installer = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("LDtkUnity.Editor.LDtkTilesetExporterUtil"))
                .FirstOrDefault(type => type != null);
            if (installer == null)
                throw new InvalidOperationException("Install com.cammin.ldtkunity 6.12.4 first.");

            var install = installer.GetMethod("UnzipToLibrary", BindingFlags.Public | BindingFlags.Static);
            if (install == null)
                throw new MissingMethodException(installer.FullName, "UnzipToLibrary");

            install.Invoke(null, null);
            Debug.Log("LDtk exporter prepared in Library/LDtkTilesetExporter. Source assets were not modified.");
        }
    }
}
