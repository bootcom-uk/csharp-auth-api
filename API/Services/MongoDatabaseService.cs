using API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace API.Services
{
    public class MongoDatabaseService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<AccessCodeRequest> _accessCodesCollection;
        private readonly IMongoCollection<RefreshToken> _refreshTokensCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public MongoDatabaseService(IMongoClient mongoClient)
        {
            _database = mongoClient.GetDatabase("Identity");
            _accessCodesCollection = _database.GetCollection<AccessCodeRequest>("AccessCodeRequest");
            _refreshTokensCollection = _database.GetCollection<RefreshToken>("RefreshToken");
            _usersCollection = _database.GetCollection<User>("User");
        }

        public async Task<Models.User?> GetUserById(ObjectId userId)
        {
            var users = await _usersCollection.FindAsync(record => record.Id == userId);
            return users.FirstOrDefault();
        }

        public async Task<Models.User?> GetUserByEmailAddress(string emailAddress)
        {
            var users = await _usersCollection.FindAsync(record => record.EmailAddress == emailAddress);
            return users.FirstOrDefault();
        }

        // Store access code with TTL index
        public async Task<AccessCodeRequest> StoreAccessCodeAsync(Models.User user, string deviceId, string audience)
        {

            var accessCodeRequest = new AccessCodeRequest()
            {
                QuickAccessCode = new Random().Next(100000, 999999).ToString(),
                DeviceId = deviceId,
                Audience = audience,
                AccessCodeExpires = DateTime.UtcNow.AddMinutes(15),
                UserId = user.Id,
                EmailAddress = user.EmailAddress,
                Id = ObjectId.GenerateNewId(),
                LoginCode = Guid.NewGuid()
            };

            await _accessCodesCollection.InsertOneAsync(accessCodeRequest);

            return accessCodeRequest;

        }

        // Validate access code and device ID
        public async Task<AccessCodeRequest> ValidateAccessCodeAsync(string accessCode, string deviceId)
        {

            var result = await _accessCodesCollection.FindAsync(record => record.QuickAccessCode  == accessCode && record.DeviceId == deviceId);

            return await result.FirstOrDefaultAsync();

        }

        // Store refresh token associated with userId and deviceId
        public async Task<RefreshToken> StoreRefreshTokenAsync(ObjectId userId, string deviceId, string audience)
        {

            var refreshToken = new RefreshToken() { 
                Audience = audience ,
                DateCreated = DateTime.UtcNow,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                DeviceId = deviceId,
                Expiry = DateTime.UtcNow.AddDays(30),
                Id = ObjectId.GenerateNewId(),  
                UserId = userId
            };

            await _refreshTokensCollection.InsertOneAsync(refreshToken);

            return refreshToken;
        }

        // Validate refresh token
        public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, string deviceId)
        {

            var result = await _refreshTokensCollection.FindAsync(record => record.Token == refreshToken && record.DeviceId == deviceId);
            return await result.FirstOrDefaultAsync();

        }

        // Remove refresh token
        public async Task RemoveRefreshTokenAsync(string refreshToken, string deviceId)
        {
            await _refreshTokensCollection.DeleteOneAsync(record => record.Token == refreshToken && record.DeviceId == deviceId);
        }

        // TTL index creation for AccessCodes collection (run on app startup)
        public async Task EnsureAccessCodeTTLIndexAsync()
        {
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromMinutes(15) };
            var indexKeys = Builders<AccessCodeRequest>.IndexKeys.Ascending("expires");

            await _accessCodesCollection.Indexes.CreateOneAsync(new CreateIndexModel<AccessCodeRequest>(indexKeys, indexOptions));
        }

        // TTL index creation for RefreshTokens collection (run on app startup)
        public async Task EnsureRefreshTokenTTLIndexAsync()
        {
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
            var indexKeys = Builders<RefreshToken>.IndexKeys.Ascending("expires");

            await _refreshTokensCollection.Indexes.CreateOneAsync(new CreateIndexModel<RefreshToken>(indexKeys, indexOptions));
        }

        public async Task<AccessCodeRequest> GetAccessCode(string code, string deviceId)
        {
            var result = await _accessCodesCollection.FindAsync(record => record.QuickAccessCode == code && record.DeviceId == deviceId);

            return await result.FirstOrDefaultAsync();

        }

        public async Task<RefreshToken> GetRefreshToken(string refreshToken, string deviceId)
        {

            var result = await _refreshTokensCollection.FindAsync(record => record.Token == refreshToken && record.DeviceId == deviceId);
            return await result.FirstOrDefaultAsync();

        }

    }

}
