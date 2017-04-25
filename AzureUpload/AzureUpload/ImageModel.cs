using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.MobileServices;

namespace AzureUpload
{
    public class ImageModel
    {
        string id;
        string name;
        string filePath;
        bool isUploaded = false;

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [JsonProperty(PropertyName = "filePath")]
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        [JsonProperty(PropertyName = "isUploaded")]
        public bool IsUploaded
        {
            get { return isUploaded; }
            set { isUploaded = value; }
        }
    }
}
