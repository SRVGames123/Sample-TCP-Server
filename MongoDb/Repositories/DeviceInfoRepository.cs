using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.MongoDb.Model.DeviceInfo;
using ModalStrikeServer.RpcServer.Exceptions.DeviceInfo;
using MongoDB.Driver;

namespace ModalStrikeServer.MongoDb.Repositories
{
    public class DeviceInfoRepository(IMongoDatabase database) : RepositoryBase<DeviceInfoModel>(database)
    {
        protected override string CollectionName => "deviceInfo";

        public async Task CreateDeviceInfoAsync(DeviceInfoModel deviceInfo)
        {
            var model = GetAsync(x => x.deviceId == deviceInfo.deviceId);
            
            if (model is not null) throw new DeviceInfoAlreadyExistException();
            
            await CreateAsync(deviceInfo);
        }

        public async Task<DeviceInfoModel> GetDeviceInfoAsync(string deviceId)
            => await GetAsync(x => x.deviceId == deviceId);

        public async Task BannedDeviceAsync(string deviceId)
        {
            var filter = FilterBuilder.Eq(x => x.deviceId, deviceId);
            var update = UpdateBuilder.Set(x => x.isBanned, true);
            
            await UpdateAsync(filter, update);
        }
    }
}