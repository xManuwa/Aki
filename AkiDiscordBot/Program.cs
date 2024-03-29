﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AkiDiscordBot.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AkiDiscordBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        LoadPrefix lp = new LoadPrefix();

        public static ulong joinedServerId;
        public static string joinedServerName;

        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        public async Task RunBotAsync()
        {
            Console.WriteLine("DiscordBot Aki, version " + Config.bot.version + "\n");

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += _client_Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, Config.bot.token);
            await _client.StartAsync();

            await _client.SetGameAsync(Config.bot.cmdPrefix + " || " + Config.bot.version);

            await Task.Delay(-1);
        }

        private Task _client_Log(LogMessage msg)
        {
            Console.WriteLine(msg);

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += Moderation.WordsFilter;

            _client.JoinedGuild += RegisterJoinAsync;
            _client.LeftGuild += RegisterLeftAsync;

            _client.UserJoined += Welcome.WelcomeMsg;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            var user = message.Author;

            EmbedBuilder embed = new EmbedBuilder();
            embed.Color = Commands.color;

            // Die Guild ID auslesen für Pfad
            var channel = message.Channel as SocketGuildChannel;
            var guild = channel.Guild.Id;
            var guildName = channel.Guild.Name;

            // Den für den Server festgelegten Prefix laden
            string cmdPrefix = lp.loadPrefix(guild);

            int argPos = 0;
            if (message.HasStringPrefix(cmdPrefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                Console.WriteLine($"[{guildName}({guild})] {user}: {message}");

                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    embed.Title = result.ErrorReason;
                    await ((ISocketMessageChannel)_client.GetChannel(channel.Id)).SendMessageAsync("", false, embed.Build());

                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        private async Task RegisterJoinAsync(SocketGuild guild)
        {
            joinedServerId = guild.Id;
            joinedServerName = guild.Name;

            Console.WriteLine("[JOIN] Aki has joined a new Server.");
            Console.WriteLine("[JOIN] Name: " + joinedServerName);
            Console.WriteLine("[JOIN] Id: " + joinedServerId);
            UserData.Data();
        }

        private async Task RegisterLeftAsync(SocketGuild guild)
        {
            joinedServerId = guild.Id;
            joinedServerName = guild.Name;

            Console.WriteLine("[LEFT] Aki has left a Server.");
            Console.WriteLine("[LEFT] Name: " + joinedServerName);
            Console.WriteLine("[LEFT] Id: " + joinedServerId);
        }
    }
}
