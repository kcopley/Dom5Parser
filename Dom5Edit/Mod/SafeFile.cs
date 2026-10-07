namespace Dom5Edit
{
    /// <summary>
    /// Writes a file so that a failure part-way never damages the existing one: the text goes to
    /// a temporary file next to it, the existing file is kept as "&lt;file&gt;.bak", then the new
    /// file takes its place.
    /// </summary>
    public static class SafeFile
    {
        public static void Write(string path, Action<StreamWriter> write, System.Text.Encoding? encoding = null)
        {
            var full = Path.GetFullPath(path);
            var tmp = full + ".tmp";
            try
            {
                using (var writer = encoding != null ? new StreamWriter(tmp, false, encoding) : new StreamWriter(tmp))
                {
                    write(writer);
                }
                // (File.Replace isn't supported on every file system, e.g. network shares)
                if (File.Exists(full))
                    File.Copy(full, full + ".bak", overwrite: true);
                File.Move(tmp, full, overwrite: true);
            }
            catch
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
                throw;
            }
        }
    }
}
