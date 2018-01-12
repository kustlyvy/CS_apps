using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live
{
    public class Parameters
    {
        public Parameters(string srv, string db, string lg, string pwd)
        {
            server = srv;
            database = db;
            login = lg;
            password = pwd;
        }
       public string server;
       public string database;
       public string login;
       public string password;
    }
}
