using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class VoucherClientService
    {
        private readonly IMongoCollection<Voucher> _voucherCollection;

        public VoucherClientService(MongoDBService mongoDBService)
        {
            _voucherCollection = mongoDBService._voucherCollection;
        }

        public async Task<List<Voucher>> GetActiveVouchersAsync()
        {
            return await _voucherCollection
                .Find(v => v.IsActive)  
                .ToListAsync();
        }

        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            return await _voucherCollection
                .Find(v => v.Code == code && v.IsActive)  
                .FirstOrDefaultAsync();
        }
        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            return await _voucherCollection.Find(v => v.IsActive && v.EndDate > DateTime.UtcNow)
                                           .ToListAsync();
        }
        public async Task AddVoucherAsync(Voucher voucher)
        {
            await _voucherCollection.InsertOneAsync(voucher);
        }

        public async Task UpdateVoucherAsync(string voucherId, Voucher updatedVoucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, new ObjectId(voucherId));
            await _voucherCollection.ReplaceOneAsync(filter, updatedVoucher);
        }
        public Voucher GetVoucherById(string voucherId)
        {
            ObjectId parsedVoucherId;
            if (!ObjectId.TryParse(voucherId, out parsedVoucherId))
            {
                return null; 
            }

            return _voucherCollection.Find(v => v.Id == parsedVoucherId).FirstOrDefault();
        }
        public async Task<Voucher> GetVoucherByIdAsync(string voucherId)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, ObjectId.Parse(voucherId));
            return await _voucherCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateVoucherUsageCountAsync(Voucher voucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id);
            var update = Builders<Voucher>.Update.Set(v => v.UsageCount, voucher.UsageCount);

            await _voucherCollection.UpdateOneAsync(filter, update);
        }


        public void UpdateVoucherUsageCount(Voucher voucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id);
            var update = Builders<Voucher>.Update.Set(v => v.UsageCount, voucher.UsageCount);
            _voucherCollection.UpdateOne(filter, update);
        }

        public async Task DeleteVoucherAsync(string voucherId)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, new ObjectId(voucherId));
            await _voucherCollection.DeleteOneAsync(filter);
        }
    }
}
