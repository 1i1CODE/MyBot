using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data.SQLite;
using System.IO;

namespace MyBot
{
    
    class Program
    {
        const string TOKEN = "5023654227:AAFkNzfg_n8X-a-77EKIizUz5Q7iX47xRi0";
        public static SQLiteConnection DB;
        static void Main(string[] args)
        {
            while(true)
            {
                try
                {
                    GetMessages().Wait();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error:" + ex);
                }
            }
            
        }
        static async Task GetMessages()
        {
            TelegramBotClient bot = new TelegramBotClient(TOKEN);
            int offset = 0;
            int timeout = 0;
            try
            {
                await bot.SetWebhookAsync("");
                while(true)
                {
                    var updates = await bot.GetUpdatesAsync(offset, timeout);

                    foreach (var update in updates)
                    {
                        var message = update.Message;
                        if (message.Text == "/start")
                        {
                            Console.WriteLine("Полученное сообщение:" + message.Text);
                            var keyboard = new ReplyKeyboardMarkup

                            {
                                Keyboard = new[] 
                                {
                               new[]
                                {
                                    new KeyboardButton("\U0001F601 дай стикер"),
                                    new KeyboardButton("Как дела?")
                                },
                               new[]
                                {
                                    new KeyboardButton("Хай"),
                                    new KeyboardButton("Как жизнь?")
                                }
                            }
                            };
                            await bot.SendTextMessageAsync(message.Chat.Id, "Привет создатель, я твой бот! " + message.Chat.Username, ParseMode.Html, false, false, 0, keyboard);
                            
                        }
                        if (message.Text == "\U0001F601 дай стикер")
                        {
                            var sticker = new FileToSend("CAACAgIAAxkBAAEDb8lhsKmYlJlmOeqWPJ9LOYoPUc3F4AAC1Q8AAkZyuUlz7diQ8HgVJSME");
                            await bot.SendStickerAsync(message.Chat.Id, sticker);
                        }
                        if (message.Text == "/reg")
                        {
                            Registration(message.Chat.Id.ToString(), message.Chat.Username.ToString());
                            await bot.SendTextMessageAsync(message.Chat.Id, "Пользователь зарегистрирован");
                        }
                       
                        if (message.Type == MessageType.DocumentMessage)
                        {
                            await SaveFile(bot, message);
                            Console.WriteLine(message.Chat.Id + "отправил файл");
                            await bot.SendTextMessageAsync(message.Chat.Id, "Ваш файл сохранен!", replyToMessageId: message.MessageId);
                        }

                        if (message.Text  == "/gerFiles")
                        {
                            await SendFile(bot, message);
                            Console.WriteLine(message.Chat.Id + " скачал свои файлы");
                            await bot.SendTextMessageAsync(message.Chat.Id, "Все файлы получены!");

                        }
                        offset = update.Id + 1;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        public static void Registration(string chatId, string username)
        {
            try 
            { 
                DB = new SQLiteConnection("Data Source=C:/Users/Юрий/source/repos/MyBot/MyBot/DB.db;");
                DB.Open();
                SQLiteCommand regcmd = DB.CreateCommand();
                regcmd.CommandText = "INSERT INTO RegUsers VALUES(@chatId, @username)";
                regcmd.Parameters.AddWithValue("@chatId", chatId);
                regcmd.Parameters.AddWithValue("@username", username);
                regcmd.ExecuteNonQuery();
                DB.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            
        }
        static async Task SaveFile(TelegramBotClient bot, Message message)
        {
            string fileid = message.Document.FileId;
            var file = await bot.GetFileAsync(fileid);
            var stream = file.FileStream;
            string filename = message.Document.FileName;
            string path = String.Format(@"C:\Users\Юрий\source\repos\MyBot\{0}\", message.From.Id);
            var di = Directory.CreateDirectory(path);

            try
            {
                using (var output = new FileStream(path + filename, FileMode.OpenOrCreate))
                {
                    await stream.CopyToAsync(output);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex);
            }
        }

        static async Task SendFile(TelegramBotClient bot, Message message)
        {
            var info = new DirectoryInfo(@"C:\Users\Юрий\source\repos\MyBot\" + message.Chat.Id);
            try
            {
                FileInfo[] fileInfoGroup = info.GetFiles();
                foreach (var fileInfo in fileInfoGroup)
                {
                    var fileStream = new FileStream(fileInfo.FullName, FileMode.Open);
                    var file = new FileToSend(fileInfo.Name, fileStream);
                    await bot.SendDocumentAsync(message.Chat.Id, file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex);
            }
        }
    }
}
