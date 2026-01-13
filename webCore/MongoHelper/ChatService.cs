using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ChatService
    {
        private readonly IMongoCollection<ChatMessage> _chatCollection;

        public ChatService(MongoDBService mongoService)
        {
            _chatCollection = mongoService._chatCollection;
        }

        public async Task<List<ConversationInfo>> GetConversationsForUserAsync(string userId, bool isSeller)
        {
            FilterDefinition<ChatMessage> filter;

            if (isSeller)
            {
                filter = Builders<ChatMessage>.Filter.Eq(m => m.SellerId, userId);
            }
            else
            {
                filter = Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, userId);
            }

            var messages = await _chatCollection.Find(filter).ToListAsync();

            Console.WriteLine($"[ChatService] GetConversations - UserId: {userId}, IsSeller: {isSeller}, Found {messages.Count} raw messages");

            if (!messages.Any())
            {
                Console.WriteLine($"[ChatService] No messages found for user {userId}");
                return new List<ConversationInfo>();
            }

            foreach (var msg in messages.Take(3))
            {
                Console.WriteLine($"[ChatService] Sample message - SellerId: {msg.SellerId}, BuyerId: {msg.BuyerId}, SenderId: {msg.SenderId}");
            }

            List<ConversationInfo> conversations = new List<ConversationInfo>();

            if (isSeller)
            {
                var grouped = messages
                    .Where(m => !string.IsNullOrEmpty(m.BuyerId))
                    .GroupBy(m => m.BuyerId);

                Console.WriteLine($"[ChatService] Seller mode - Found {grouped.Count()} buyer groups");

                foreach (var group in grouped)
                {
                    var lastMessage = group.OrderByDescending(m => m.SentAt).FirstOrDefault();
                    if (lastMessage != null)
                    {
                        conversations.Add(new ConversationInfo
                        {
                            OtherUserId = group.Key,
                            LastMessage = lastMessage.Message,
                            LastMessageTime = lastMessage.SentAt,
                            MessageCount = group.Count()
                        });

                        Console.WriteLine($"[ChatService] Added conversation with Buyer: {group.Key} ({group.Count()} messages)");
                    }
                }
            }
            else
            {
                var grouped = messages
                    .Where(m => !string.IsNullOrEmpty(m.SellerId))
                    .GroupBy(m => m.SellerId);

                Console.WriteLine($"[ChatService] Buyer mode - Found {grouped.Count()} seller groups");

                foreach (var group in grouped)
                {
                    var lastMessage = group.OrderByDescending(m => m.SentAt).FirstOrDefault();
                    if (lastMessage != null)
                    {
                        conversations.Add(new ConversationInfo
                        {
                            OtherUserId = group.Key,
                            LastMessage = lastMessage.Message,
                            LastMessageTime = lastMessage.SentAt,
                            MessageCount = group.Count()
                        });

                        Console.WriteLine($"[ChatService] Added conversation with Seller: {group.Key} ({group.Count()} messages)");
                    }
                }
            }

            Console.WriteLine($"[ChatService] Total conversations created: {conversations.Count}");

            return conversations.OrderByDescending(c => c.LastMessageTime).ToList();
        }

        public async Task<List<ChatMessage>> GetMessagesBetweenUsersAsync(string buyerId, string sellerId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, buyerId),
                Builders<ChatMessage>.Filter.Eq(m => m.SellerId, sellerId)
            );

            var messages = await _chatCollection.Find(filter)
                                        .SortBy(m => m.SentAt)
                                        .ToListAsync();

            Console.WriteLine($"[ChatService] GetMessages - BuyerId: {buyerId}, SellerId: {sellerId}, Found {messages.Count} messages");

            return messages;
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.SellerId))
            {
                throw new ArgumentException("SellerId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(message.BuyerId))
            {
                throw new ArgumentException("BuyerId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(message.SenderId))
            {
                throw new ArgumentException("SenderId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(message.ReceiverId))
            {
                throw new ArgumentException("ReceiverId cannot be null or empty");
            }

            Console.WriteLine($"[ChatService] Adding message - SellerId: {message.SellerId}, BuyerId: {message.BuyerId}, SenderId: {message.SenderId}, RelatedOrderId: {message.RelatedOrderId}");

            await _chatCollection.InsertOneAsync(message);

            Console.WriteLine($"[ChatService] Message added successfully with Id: {message.Id}");
        }

        public async Task<bool> HasConversationAsync(string buyerId, string sellerId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, buyerId),
                Builders<ChatMessage>.Filter.Eq(m => m.SellerId, sellerId)
            );

            var count = await _chatCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<ChatMessage> GetLastMessageAsync(string buyerId, string sellerId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, buyerId),
                Builders<ChatMessage>.Filter.Eq(m => m.SellerId, sellerId)
            );

            return await _chatCollection.Find(filter)
                                       .SortByDescending(m => m.SentAt)
                                       .FirstOrDefaultAsync();
        }
    }

    public class ConversationInfo
    {
        public string OtherUserId { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int MessageCount { get; set; }
    }
}