using System.Runtime.InteropServices;
using SQLitePCL;

namespace DoqLite;

public static class SetupSqlite
{
    public static void Init()
    {
        SQLite3Provider_dynamic_cdecl
            .Setup("sqlite3", new NativeLibraryAdapter("sqlite3"));
        raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
    }

    class NativeLibraryAdapter : IGetFunctionPointer
    {
        readonly IntPtr _library;

        public NativeLibraryAdapter(string name)
            => _library = NativeLibrary.Load(name);

        public IntPtr GetFunctionPointer(string name)
            => NativeLibrary.TryGetExport(_library, name, out var address)
                ? address
                : IntPtr.Zero;
    }

}