using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActivityScheduler.Data.Models.Settings
{
    public class SettingsManager
    {

        private IAsyncRepositoryT<SettingStorageUnit> _repo;

        public SettingsManager(IAsyncRepositoryT<SettingStorageUnit> repo)
        {
            _repo = repo;
        }

        public SettingsData GetSettings()
        {
            SettingsData data = new SettingsData();

            List<SettingStorageUnit> lst = _repo.GetAllAsync().Result.ToList();

            lst.ForEach(x => {
                ObjectParameters.SetObjectParameter(data, x.Name, x.Value);
                });

            return data;
        }
        public void SaveSettings(SettingsData data)
        {
            ClearSettingsData();

            List<ObjectParameters.ObjectParameter> prms = ObjectParameters.GetObjectParameters(data);

            foreach (ObjectParameters.ObjectParameter param in prms)
            {
                var unit = new SettingStorageUnit() { Name = param.Name, Value = param.Value.ToString() };
                _repo.AddAsync(unit);
            }
        }

        public void ClearSettingsData()
        {
            List<SettingStorageUnit> lst = _repo.GetAllAsync().Result.ToList();
            lst.ForEach((x) => { _repo.DeleteAsync(x.Id); });
        }


    }
}
