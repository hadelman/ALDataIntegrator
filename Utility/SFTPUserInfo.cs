using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tamir.SharpSsh.jsch;

namespace ALDataIntegrator.Utility
{
    public class SFTPUserInfo : UserInfo
    {
        string _passwd = string.Empty;
        public SFTPUserInfo() { _passwd = string.Empty; }
        public SFTPUserInfo(string pwd) { _passwd = pwd; }
        public String getPassword() { return _passwd; }

        public string Password
        {
            set { _passwd = value; }
            get { return _passwd; }
        }


        public string getPassphrase()
        {
            return null;
        }

        public bool promptPassword(string message)
        {
            return true;
        }

        public bool promptPassphrase(string message)
        {
            return true;
        }

        public bool promptYesNo(string message)
        {
            return true;
        }

        public void showMessage(string message)
        {
           
        }
    }
}
