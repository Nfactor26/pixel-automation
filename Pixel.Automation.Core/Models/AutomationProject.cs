using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [Serializable]
    [DataContract]
    public class AutomationProject : NotifyPropertyChanged, INotifyDataErrorInfo
    {
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; }

        string name;
        [DataMember(IsRequired = true, Order = 20)]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
                ValidateProperty(nameof(Name));
            }
        }

        [DataMember(Order = 30)]
        public ProjectType AutomationProjectType { get; set; }

        [DataMember(IsRequired = true, Order = 40)]
        public DateTime LastOpened { get; set; }

        [DataMember(IsRequired = true)]
        public List<Version> AvailableVersions { get; set; }

        [DataMember(IsRequired =  true)]
        public Version ActiveVersion { get; set; }
        
        [DataMember(IsRequired = true)]
        public Version DeployedVersion { get; set; }         

        public AutomationProject()
        {
           
        }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext sc)
        {
            //location = ".\\Automations\\";         
            if (string.IsNullOrEmpty(this.ProjectId))
                ProjectId = Guid.NewGuid().ToString();
            ErrorsChanged = delegate { };
            this.propertyErrors = new Dictionary<string, List<string>>();          
        }
             

        #region INotifyDataErrorInfo

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };

        Dictionary<string, List<string>> propertyErrors = new Dictionary<string, List<string>>();
        public bool HasErrors
        {
            get
            {
                return propertyErrors.Count > 0;
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyErrors.ContainsKey(propertyName))
                return propertyErrors[propertyName];
            return default(IEnumerable);
        }


        void ValidateProperty(string propertyName)
        {
            List<string> errors = new List<string>();
            switch (propertyName)
            {
                case "Name":

                    if (string.IsNullOrEmpty(this.name) || string.IsNullOrWhiteSpace(this.name))
                    {
                        errors.Add("Invalid Name.");
                    }
                    if (this.Name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                    {
                        //string invalidChars = string.Empty;
                        //foreach (char c in this.name)
                        //    if (System.IO.Path.GetInvalidFileNameChars().Contains(c))
                        //        invalidChars += c + ";";
                        errors.Add("Invalid characters in name.");
                    }
                    //if(System.IO.Directory.EnumerateDirectories(location).Any(d=>d.ToLower().Equals(this.name.ToLower())))
                    //{
                    //    errors.Add($"A project already exists with name : {this.name}");
                    //}

                    break;              

            }
            if (propertyErrors == null)
                propertyErrors = new Dictionary<string, List<string>>();
            if (errors.Count == 0)
            {
                if (propertyErrors.ContainsKey(propertyName))
                    propertyErrors.Remove(propertyName);
                return;
            }
            if (!propertyErrors.ContainsKey(propertyName))
                propertyErrors.Add(propertyName, errors);
            else
                propertyErrors[propertyName] = errors;
            //this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));

        }

        #endregion INotifyDataErrorInfo

    }
}
