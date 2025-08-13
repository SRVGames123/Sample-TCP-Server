using ModalStrikeServer.MongoDb.Core;

namespace ModalStrikeServer.MongoDb.Model.DeviceInfo
{
    public class DeviceInfoModel : ModelBase
    {
        public string deviceId { get; set; }
        public string deviceName { get; set; }
        public bool isBanned { get; set; }
    }
}