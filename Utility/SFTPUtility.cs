using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;

namespace ALDataIntegrator.Utility
{
    internal class SFTPUtility
    {
        private readonly Sftp _wrapper = new Sftp(Properties.Settings.Default.SFTPHost, Properties.Settings.Default.SFTPUser, Properties.Settings.Default.SFTPPass);

        private readonly string _defaultDirectory;

        internal SFTPUtility(string defaultDirectory)
        {
            _defaultDirectory = defaultDirectory;
        }

        internal List<string> GetFileNamesFromDefaultDirectory()
        {
            OpenSFTPConnection();

            var files = _wrapper.GetFileList(_defaultDirectory);

            CloseSFTPConnection();

            var result = files.Cast<string>().ToList();

            return result;
        }

        internal void PutFile(string fileName)
        {
            try
            {
                OpenSFTPConnection();

                var fromFile = Path.Combine(Properties.Settings.Default.StagingDirectory, fileName);
                var toFile = Path.Combine(_defaultDirectory, fileName);

                _wrapper.Put(fromFile, toFile);
            }
            finally
            {
                CloseSFTPConnection();
            }
        }

        internal void CopyFile(string fileName)
        {
            try
            {
                OpenSFTPConnection();

                var fromFile = Path.Combine(_defaultDirectory, fileName);

                // create the staging directory if it does not exist
                if (!Directory.Exists(Properties.Settings.Default.StagingDirectory))
                    Directory.CreateDirectory(Properties.Settings.Default.StagingDirectory);

                var toFile = Path.Combine(Properties.Settings.Default.StagingDirectory, fileName);

                _wrapper.Get(fromFile, toFile);
            }
            finally
            {
                CloseSFTPConnection();
            }
        }

        // there is no built in way to delete a file using this utility, so we have to hack a bit...
        // info found here:  http://revdoniv.wordpress.com/2010/09/11/how-to-delete-an-sftp-file-in-net/
        internal void DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_defaultDirectory, fileName);

            try
            {
                var session = new JSch().getSession(Properties.Settings.Default.SFTPUser, Properties.Settings.Default.SFTPHost, Properties.Settings.Default.SFTPPort);
                
                // you would think this would work, but nope... it doesnt...
                //session.setPassword(Properties.Settings.Default.SFTPPass);

                // you have to override this silly class
                session.setUserInfo(new SFTPUserInfo(Properties.Settings.Default.SFTPPass));

                session.connect();
                var sftpChannel = (ChannelSftp)session.openChannel("sftp");
                sftpChannel.connect();

                sftpChannel.rm(filePath);

                sftpChannel.disconnect();
                session.disconnect();
            }
            catch (Exception ex)
            {
                GlobalContext.Log("Unable to delete file from SFTP", true);
                GlobalContext.Log(ex.ToString(), true);
            }
        }

        private void OpenSFTPConnection()
        {
            _wrapper.Connect(Properties.Settings.Default.SFTPPort);

            if (!_wrapper.Connected)
                GlobalContext.ExitApplication(string.Format("Unable to connect to FTP Server {0}:{1} using credentials user:{2}, pass:{3}",
                    Properties.Settings.Default.SFTPHost, Properties.Settings.Default.SFTPPort,
                    Properties.Settings.Default.SFTPUser, Properties.Settings.Default.SFTPPass), 1);
        }

        private void CloseSFTPConnection()
        {
            if (_wrapper != null)
                _wrapper.Close();
        }
    }
}
