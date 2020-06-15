﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Persistence.Respository
{
    public interface IMongoDbSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string SessionsCollectionName { get; set; }
    }

    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string SessionsCollectionName { get; set; }
    }  
    
}
