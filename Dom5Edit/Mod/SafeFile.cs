namespace Dom5Edit
{
    /// <summary>
    /// Writes a file so that a failure part-way never damages the existing one: the text goes to
    /// a temporary file next to it, the existing file is kept as "&lt;file&gt;.bak", then the new
    /// file takes its place.
    /// </summary>
    public static class SafeFile
    {
        /// <summary>
        /// Writes to a temporary file, checks it (<paramref name="verify"/> throws if it's wrong:
        /// then the file at <paramref name="path"/> is left as it was, and the attempt is kept in
        /// the backups folder for a look), then puts it in place, the previous file kept as .bak.
        /// </summary>
        public static void Write(string path, Action<StreamWriter> write, System.Text.Encoding? encoding = null, Action<string>? verify = null)
        {
            var full = Path.GetFullPath(path);
            var tmp = full + ".tmp";
            try
            {
                using (var writer = encoding != null ? new StreamWriter(tmp, false, encoding) : new StreamWriter(tmp))
                {
                    write(writer);
                }
                if (verify != null)
                {
                    try
                    {
                        verify(tmp);
                    }
                    catch (Exception ex)
                    {
                        var dir = ModBackups.FolderOf(full);
                        Directory.CreateDirectory(dir);
                        var kept = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}_failed-save.dm.txt");
                        File.Move(tmp, kept, overwrite: true);
                        throw new IOException($"{ex.Message} Nothing was changed: {Path.GetFileName(full)} is as it was. The attempted save is kept at {kept}.", ex);
                    }
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
