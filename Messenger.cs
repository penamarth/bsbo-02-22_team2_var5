using System;
using System.Collections.Generic;

// Общие модули
namespace UnifiedMessagingSystem
{
    // Пользователи
    abstract class User
    {
        public string UserName { get; set; }

        protected User(string userName)
        {
            UserName = userName;
        }

        public abstract void DisplayInfo();
    }

    class Friend : User
    {
        public Friend(string userName) : base(userName) { }

        public override void DisplayInfo()
        {
            Console.WriteLine($"Friend: {UserName}");
        }
    }

    class BlockedUser : User
    {
        public BlockedUser(string userName) : base(userName) { }

        public override void DisplayInfo()
        {
            Console.WriteLine($"Blocked: {UserName}");
        }
    }

    class FriendsList
    {
        private List<Friend> friends = new List<Friend>();

        public void AddFriend(Friend friend)
        {
            friends.Add(friend);
            Console.WriteLine($"Added friend: {friend.UserName}");
        }

        public void DisplayFriends()
        {
            Console.WriteLine("\nList of Friends:");
            foreach (var friend in friends)
            {
                friend.DisplayInfo();
            }
        }
    }

    class BlockedList
    {
        private List<BlockedUser> blockedUsers = new List<BlockedUser>();

        public void AddBlockedUser(BlockedUser blockedUser)
        {
            blockedUsers.Add(blockedUser);
            Console.WriteLine($"Blocked user: {blockedUser.UserName}");
        }

        public void DisplayBlockedUsers()
        {
            Console.WriteLine("\nList of Blocked Users:");
            foreach (var blockedUser in blockedUsers)
            {
                blockedUser.DisplayInfo();
            }
        }
    }

    // Сообщения
    public class Message
    {
        public int MessageID { get; private set; }
        public int SenderID { get; private set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Message(int messageID, int senderID, string content)
        {
            MessageID = messageID;
            SenderID = senderID;
            Content = content;
            Timestamp = DateTime.Now;
        }
    }

    public class Chat
    {
        public int ChatID { get; private set; }
        public List<int> Participants { get; private set; }
        public List<Message> Messages { get; private set; }
        public List<Message> PinnedMessages { get; private set; }

        public Chat(int chatID, List<int> participants)
        {
            ChatID = chatID;
            Participants = participants;
            Messages = new List<Message>();
            PinnedMessages = new List<Message>();
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        public void PinMessage(int messageID)
        {
            var message = Messages.Find(m => m.MessageID == messageID);
            if (message != null && !PinnedMessages.Contains(message))
            {
                PinnedMessages.Add(message);
                Console.WriteLine($"Message {messageID} pinned in chat {ChatID}.");
            }
            else
            {
                Console.WriteLine($"Message {messageID} not found or already pinned.");
            }
        }

        public void DisplayPinnedMessages()
        {
            Console.WriteLine($"Pinned messages in chat {ChatID}:");
            foreach (var message in PinnedMessages)
            {
                Console.WriteLine($"[{message.Timestamp}] {message.Content}");
            }
        }
    }

    public class Messenger
    {
        private static Messenger _instance;
        private Dictionary<int, Chat> Chats;

        private Messenger()
        {
            Chats = new Dictionary<int, Chat>();
        }

        public static Messenger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Messenger();
                }
                return _instance;
            }
        }

        public List<Chat> GetUserChats(int userID)
        {
            List<Chat> userChats = new List<Chat>();
            foreach (var chat in Chats.Values)
            {
                if (chat.Participants.Contains(userID))
                {
                    userChats.Add(chat);
                }
            }
            return userChats;
        }

        public List<Message> GetMessages(int chatID)
        {
            if (Chats.ContainsKey(chatID))
            {
                return Chats[chatID].Messages;
            }
            throw new Exception("Chat not found");
        }

        public void SendMessage(int chatID, int senderID, string messageContent)
        {
            if (Chats.ContainsKey(chatID))
            {
                var message = new Message(new Random().Next(1, 10000), senderID, messageContent);
                Chats[chatID].AddMessage(message);
                Console.WriteLine($"Message sent to chat {chatID} by user {senderID}");
            }
            else
            {
                throw new Exception("Chat not found");
            }
        }

        public int CreateChat(List<int> participants)
        {
            int newChatID = new Random().Next(1, 10000);
            Chats[newChatID] = new Chat(newChatID, participants);
            Console.WriteLine($"Chat {newChatID} created.");
            return newChatID;
        }

        public void PinMessage(int chatID, int messageID)
        {
            if (Chats.ContainsKey(chatID))
            {
                Chats[chatID].PinMessage(messageID);
            }
            else
            {
                Console.WriteLine("Chat not found.");
            }
        }

        public void DisplayPinnedMessages(int chatID)
        {
            if (Chats.ContainsKey(chatID))
            {
                Chats[chatID].DisplayPinnedMessages();
            }
            else
            {
                Console.WriteLine("Chat not found.");
            }
        }
    }

    public class MessengerProxy
    {
        private Messenger messenger;
        private Dictionary<int, List<Message>> cacheMessages;
        private Dictionary<int, List<Chat>> cacheChats;

        public MessengerProxy()
        {
            messenger = Messenger.Instance;
            cacheMessages = new Dictionary<int, List<Message>>();
            cacheChats = new Dictionary<int, List<Chat>>();
        }

        public List<Chat> GetUserChats(int userID)
        {
            if (!cacheChats.ContainsKey(userID))
            {
                Console.WriteLine("Cache miss for user chats. Fetching from Messenger.");
                var chats = messenger.GetUserChats(userID);
                cacheChats[userID] = chats;
            }
            else
            {
                Console.WriteLine("Cache hit for user chats.");
            }
            return cacheChats[userID];
        }

        public List<Message> GetMessages(int chatID)
        {
            if (!cacheMessages.ContainsKey(chatID))
            {
                Console.WriteLine("Cache miss for chat messages. Fetching from Messenger.");
                var messages = messenger.GetMessages(chatID);
                cacheMessages[chatID] = messages;
            }
            else
            {
                Console.WriteLine("Cache hit for chat messages.");
            }
            return cacheMessages[chatID];
        }

        public void SendMessage(int chatID, int senderID, string messageContent)
        {
            messenger.SendMessage(chatID, senderID, messageContent);
            if (cacheMessages.ContainsKey(chatID))
            {
                Console.WriteLine("Invalidating cache for chat messages due to new message.");
                cacheMessages.Remove(chatID);
            }
        }

        public void PinMessage(int chatID, int messageID)
        {
            messenger.PinMessage(chatID, messageID);
        }

        public void DisplayPinnedMessages(int chatID)
        {
            messenger.DisplayPinnedMessages(chatID);
        }
    }

    // Уведомления
    public abstract class Notification
    {
        public abstract void Send(string message);
    }

    public class EmailNotification : Notification
    {
        public override void Send(string message)
        {
            Console.WriteLine($"Email notification sent: {message}");
        }
    }

    public class SMSNotification : Notification
    {
        public override void Send(string message)
        {
            Console.WriteLine($"SMS notification sent: {message}");
        }
    }

    public class NotificationService
    {
        private List<Notification> _notifications = new List<Notification>();

        public void AddNotification(Notification notification)
        {
            _notifications.Add(notification);
        }

        public void NotifyAll(string message)
        {
            foreach (var notification in _notifications)
            {
                notification.Send(message);
            }
        }
    }

    // Звонки
    public abstract class Call
    {
        public string Caller { get; set; }
        public string Receiver { get; set; }

        protected Call(string caller, string receiver)
        {
            Caller = caller;
            Receiver = receiver;
        }

        public abstract void StartCall();

        public void EndCall()
        {
            Console.WriteLine($"Call between {Caller} and {Receiver} ended.");
        }
    }

    public class VoiceCall : Call
    {
        public VoiceCall(string caller, string receiver) : base(caller, receiver) { }

        public override void StartCall()
        {
            Console.WriteLine($"Voice call started from {Caller} to {Receiver}.");
        }
    }

    public class VideoCall : Call
    {
        public VideoCall(string caller, string receiver) : base(caller, receiver) { }

        public override void StartCall()
        {
            Console.WriteLine($"Video call started from {Caller} to {Receiver}.");
        }
    }

    // Отправка файлов
    public abstract class FileSender
    {
        public string FilePath { get; set; }

        protected FileSender(string filePath)
        {
            FilePath = filePath;
        }

        public abstract void Send();
    }

    public class TextFileSender : FileSender
    {
        public TextFileSender(string filePath) : base(filePath) { }

        public override void Send()
        {
            Console.WriteLine($"Text file sent: {FilePath}");
        }
    }

    public class ImageFileSender : FileSender
    {
        public ImageFileSender(string filePath) : base(filePath) { }

        public override void Send()
        {
            Console.WriteLine($"Image file sent: {FilePath}");
        }
    }

    public class VideoFileSender : FileSender
    {
        public VideoFileSender(string filePath) : base(filePath) { }

        public override void Send()
        {
            Console.WriteLine($"Video file sent: {FilePath}");
        }
    }

    public class FileSenderManager
    {
        public void SendFile(FileSender fileSender)
        {
            fileSender.Send();
            Console.WriteLine($"File {fileSender.FilePath} sent successfully.");
        }
    }

    // Основной класс программы
    class Program
    {
        static void Main(string[] args)
        {
            MessengerProxy messengerProxy = new MessengerProxy();
            NotificationService notificationService = new NotificationService();
            notificationService.AddNotification(new EmailNotification());
            notificationService.AddNotification(new SMSNotification());

            // Создание чата
            var chatID = Messenger.Instance.CreateChat(new List<int> { 1, 2 });

            // Отправка сообщения
            messengerProxy.SendMessage(chatID, 1, "Hello!");

            // Получение сообщений (демонстрация кеша)
            var messages1 = messengerProxy.GetMessages(chatID); // Cache miss
            var messages2 = messengerProxy.GetMessages(chatID); // Cache hit

            // Закрепление сообщения
            messengerProxy.PinMessage(chatID, messages1[0].MessageID);
            messengerProxy.DisplayPinnedMessages(chatID);

            // Уведомление
            notificationService.NotifyAll("New message sent.");

            // Звонок
            Call call = new VideoCall("Alice", "Bob");
            call.StartCall();
            call.EndCall();

            // Отправка файлов
            FileSenderManager fileManager = new FileSenderManager();
            FileSender textFile = new TextFileSender("doc.txt");
            fileManager.SendFile(textFile);
        }
    }
}
