﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AkiDiscordBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        LoadPrefix lp = new LoadPrefix();

        public static Color color = new Color(0xFF7700);
        string sPrefix = Config.bot.cmdPrefix;
        //string gif01 = @"Resources/Gifs/gif01.gif";
        //Uri gif01 = new Uri("file:///Resources/Gifs/gif01.gif");

        #region useful
        [Command("help")]
        [Summary("Lässt alle Commands ausgeben")]
        public async Task Help([Summary("Gibt aus, wie der Command verwendet wird")]string helpCommand = null)
        {
            ulong guild = Context.Guild.Id;
            string prefix = lp.loadPrefix(guild);

            if(helpCommand == null)
            { 
                // Speichert alle commands in einer Liste
                List<CommandInfo> commands = Program._commands.Commands.ToList();

                string[] commandsArray = new string[commands.Count+1];
                int count = 0;

                var user = Context.User as SocketGuildUser;

                foreach (CommandInfo command in commands)
                {
                    string commandName = command.Name ?? "Kein Command gefunden";
                    string commandInfo = command.Summary ?? "Keine Beschreibung verfügbar";
                    //string commandAttribute = ;
                    //Console.WriteLine($"{commandName}: {commandAttribute}");
                    //if(user.GuildPermissions.Administrator)
                    // TODO: Commands nicht ausgeben, wenn der User keine Rechte dazu hat
                    commandsArray[count] += $"> ● **{prefix}{commandName}**:  {commandInfo}\n";
                    count++;
                }

                await Context.User.SendMessageAsync(string.Join("", commandsArray));
                //await Context.Channel.SendMessageAsync(string.Join("", commandsArray));
            }
            else
            {
                // TODO: Command usage anzeigen
            }
        }
        
        [Command("version")]
        [Summary("Gibt die aktuelle Version aus")]
        public async Task Version()
        {
            // Version pattern:
            // [Main].[Year].[Update].[Patch]

            var embed = new EmbedBuilder()
            {
                Title = "Version",
                Description = Config.bot.version,
                Color = color
            }.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("prefix")]
        [Summary("Zeigt den Prefix an oder ändert ihn")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Prefix([Summary("Zu welchem Prefix der momentane Prefix geändert werden soll")]string prefix = null)
        {
            ulong guild = Context.Guild.Id;
            string path = UserData.userDataFolder + UserData.userDataFolder02 + guild + "/" + UserData.prefixData;

            string currentPrefix = File.ReadAllText(path);

            if (prefix != null)
            {
                // Überschreibe alten Prefix mit neuem Prefix
                currentPrefix = prefix;

                File.Delete(path);
                File.WriteAllText(path, prefix);

                await Program._client.SetGameAsync(prefix + "help for commands");
            }

            var embed = new EmbedBuilder()
            {
                Title = "Festgelegter Prefix",
                Description = currentPrefix,
                Color = color
            }.Build();

            await ReplyAsync(embed: embed);          
        }

        [Command("kick")]
        [Summary("Kickt einen Spieler")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, [Remainder]string reason = "Keine Begründung angegeben")
        {
            await user.KickAsync(reason);
        }

        [Command("ban")]
        [Summary("Bannt einen Spieler")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, int msgRemoveDays = 1, [Remainder]string reason = "Keine Begründung angegeben")
        {
            await user.BanAsync(msgRemoveDays, reason);
        }
        
        [Command("accept")]
        [Summary("Akzeptiere das Regelwerk")]
        public async Task Accept()
        {
            var wavingHand = new Emoji("👋");

            var user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "User");

            if(!user.Roles.Contains(role))
            {
                await (user as IGuildUser).AddRoleAsync(role);
                await Context.Message.AddReactionAsync(wavingHand);
            }
        }
        
        #endregion useful



        #region fun
        [Command("hug")]
        [Summary("Umarmt einen Spieler")]
        public async Task Hug(string receiver)
        {
            string[] entries = { };
            string stringHugs = "0";
            string stringPats = "0";

            bool alreadyExists = false;
            bool addNumber = false;

            int index = 0;
            int indexSave = 0;

            ulong guild = Context.Guild.Id;

            string sender = "<@" + Context.User.Id + ">";
            string server = "<@" + Context.Channel.Name + ">";

            // Verfügbare Gifs
            string[] gif =
            {
                "http://25.media.tumblr.com/tumblr_ma7l17EWnk1rq65rlo1_500.gif",
                "https://38.media.tumblr.com/b004f301143edad269aa1d88d0f1e245/tumblr_mx084htXKO1qbvovho1_500.gif",
                "https://31.media.tumblr.com/f1f87c6005c580ad61155ea9e26c6d88/tumblr_inline_nanykpgO701s6lw3t.gif",
                "https://media1.tenor.com/images/49a21e182fcdfb3e96cc9d9421f8ee3f/tenor.gif?itemid=3532079",
                "https://gifimage.net/wp-content/uploads/2017/09/anime-nuzzle-gif-4.gif",
                "https://media2.giphy.com/media/mZQZ4aBMgM3Kg/200.webp?cid=790b7611138f677eb9ea9872067cd4d285d4144931c23578&rid=200.webp"
            };

            // Zufallszahl, um ein zufälliges Gif auszuwählen
            Random rnd = new Random();
            int i = rnd.Next(0, gif.Length);

            var embed = new EmbedBuilder() { Color = color };
            string path = UserData.userDataFolder + "/" + UserData.userDataFolder02 + "/" + guild + "/" + UserData.userDataFile;

            #region Anzahl Umarmungen
            // Anzahl der bisherigen Umarmungen auslesen
            List<UserData> data = new List<UserData>();
            List<string> lines = File.ReadAllLines(path).ToList();

            // Führe den Code für jede Zeile aus
            foreach (string line in lines)
            {
                entries = line.Split(';');

                UserData newData = new UserData();

                newData.Sender = entries[0];
                newData.Receiver = entries[1];
                newData.Hugs = entries[2];
                newData.Pats = entries[3];

                data.Add(newData);

                if (entries[0] == sender)
                {
                    if (entries[1] == receiver) // Sender und Receiver stehen in der selben Zeile
                    {
                        // Zu Hugs +1 hinzufügen
                        int intHugs = Convert.ToInt32(entries[2]);
                        intHugs++;

                        stringHugs = Convert.ToString(intHugs);
                        stringPats = entries[3]; // Speichert die Pats vorübergehend

                        // Speichert die Position der Zeile
                        indexSave = index;

                        // Verhindert, dass der User neu angelegt wird
                        addNumber = true;
                        alreadyExists = true;

                    } else { alreadyExists = false; } // Sender und Receiver stehen NICHT in der selben Zeile
                } else { alreadyExists = false; } // Sender wurde nicht gefunden

                index++;
            }

            if (alreadyExists == false && addNumber == false)
            {
                embed.WithFooter("Deine erste Umarmung! Wie schön.");

                // Neuen User in .txt Datei hinzufügen
                data.Add(new UserData { Sender = sender, Receiver = receiver, Hugs = "1", Pats = "1" });
                List<string> output = new List<string>();

                foreach (var user in data)
                    output.Add($"{ user.Sender };{ user.Receiver };{ user.Hugs };{ user.Pats }");

                File.WriteAllLines(path, output);
            }
            else if(addNumber == true)
            {
                embed.WithFooter($"Schon { stringHugs } Umarmungen!");

                // Zeile neu hinzufügen
                data.Add(new UserData { Sender = sender, Receiver = receiver, Hugs = stringHugs, Pats = stringPats });
                List<string> output = new List<string>();

                foreach (var user in data)
                    output.Add($"{ user.Sender };{ user.Receiver };{ user.Hugs };{ user.Pats }");

                File.WriteAllLines(path, output);

                // Alte Zeile löschen
                lines = File.ReadAllLines(path).ToList(); // Alle Zeilen einlesen
                lines.RemoveAt(indexSave); // Zeile an bestimmter Position löschen
                File.WriteAllLines(path, lines.ToArray()); // Alle Zeilen neu schreiben und neue Zeile unten hinzufügen
            }
            // // // // //
            #endregion Anzahl Umarmungen

            embed.WithDescription(sender + " umarmt " + receiver);
            embed.WithImageUrl(gif[i]);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("pat")]
        [Summary("Streichelt einen Spieler")]
        public async Task Pat(string receiver)
        {
            string[] entries = { };
            string stringHugs = "0";
            string stringPats = "0";

            bool alreadyExists = false;
            bool addNumber = false;

            int index = 0;
            int indexSave = 0;

            ulong guild = Context.Guild.Id;

            string sender = "<@" + Context.User.Id + ">";
            string[] gif =
            {
                "https://33.media.tumblr.com/72d640d7c1bb765420783a9b9cbee13c/tumblr_nxjapoyGms1u86t2qo1_540.gif",
                "https://media1.tenor.com/images/da8f0e8dd1a7f7db5298bda9cc648a9a/tenor.gif?itemid=12018819",
                "https://pa1.narvii.com/6400/8685249d3f096bae8cdd976c1b33513c5dc415b2_hq.gif",
                "https://archive-media-0.nyafuu.org/c/image/1483/55/1483553008493.gif",
                "https://media1.tenor.com/images/5466adf348239fba04c838639525c28a/tenor.gif",
                "https://thumbs.gfycat.com/SamePopularFinch.webp",
                "https://thumbs.gfycat.com/NauticalDampJerboa.webp"
            };

            Random rnd = new Random();
            int i = rnd.Next(0, gif.Length);

            var embed = new EmbedBuilder() { Color = color };
            string path = UserData.userDataFolder + "/" + UserData.userDataFolder02 + "/" + guild + "/" + UserData.userDataFile;

            #region Anzahl Pats
            // Anzahl der Umarmungen auslesen
            List<UserData> data = new List<UserData>();
            List<string> lines = File.ReadAllLines(path).ToList();

            foreach (string line in lines)
            {
                entries = line.Split(';');

                UserData newData = new UserData();

                newData.Sender = entries[0];
                newData.Receiver = entries[1];
                newData.Hugs = entries[2];
                newData.Pats = entries[3];

                data.Add(newData);

                if (entries[0] == sender)
                {
                    if (entries[1] == receiver) // Sender und Receiver stehen in der selben Zeile
                    {
                        // Zu Hugs +1 hinzufügen
                        int intPats = Convert.ToInt32(entries[3]);
                        intPats++;

                        stringPats = Convert.ToString(intPats);
                        stringHugs = entries[2]; // Speichert die Pats vorübergehend

                        indexSave = index;

                        // Verhindert, dass der User neu angelegt wird
                        addNumber = true;
                        alreadyExists = true;
                    }
                    else { alreadyExists = false; } // Sender und Receiver stehen NICHT in der selben Zeile
                }
                else { alreadyExists = false; } // Sender wurde nicht gefunden

                index++;
            }

            if (alreadyExists == false && addNumber == false)
            {
                embed.WithFooter("Deine erste Streicheleinheit! Wie schön.");

                // Neuen User in .txt Datei hinzufügen
                data.Add(new UserData { Sender = sender, Receiver = receiver, Hugs = "1", Pats = "1" });
                List<string> output = new List<string>();

                foreach (var user in data)
                    output.Add($"{ user.Sender };{ user.Receiver };{ user.Hugs };{ user.Pats }");

                File.WriteAllLines(path, output);
            }
            else if (addNumber == true)
            {
                embed.WithFooter($"Schon { stringPats } Pats!");

                // Zeile neu hinzufügen
                data.Add(new UserData { Sender = sender, Receiver = receiver, Hugs = stringHugs, Pats = stringPats });
                List<string> output = new List<string>();

                foreach (var user in data)
                    output.Add($"{ user.Sender };{ user.Receiver };{ user.Hugs };{ user.Pats }");

                File.WriteAllLines(path, output);

                // Alte Zeile löschen
                lines = File.ReadAllLines(path).ToList();
                lines.RemoveAt(indexSave);
                File.WriteAllLines(path, lines.ToArray());
            }
            // // // // //
            #endregion Anzahl Pats

            embed.WithDescription(sender + " streichelt " + receiver);
            embed.WithImageUrl(gif[i]);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
        
        [Command("lick")]
        [Summary("Mhhhh, lecker")]
        public async Task Lick(string receiver)
        {
            string sender = "<@" + Context.User.Id + ">";
            string[] gif =
            {
                //gif01,
                "http://pa1.narvii.com/5956/3f5108ea23a4cb0ac5ab2ae31e2c7634531cf786_hq.gif",
                "https://i.giphy.com/media/5QhTbwPxhCC7S/200.webp",
                "http://66.media.tumblr.com/tumblr_mdilccZR9u1r41eqro1_500.gif"
                //"https://em.wattpad.com/fc7f2206597d839dd295657afa652b51d49b6a7a/68747470733a2f2f73332e616d617a6f6e6177732e636f6d2f776174747061642d6d656469612d736572766963652f53746f7279496d6167652f5238675737526b684753494979673d3d2d3337343638363337342e313461346362313438633832663037663433333236343030303037362e676966"
            };

            Random rnd = new Random();
            int i = rnd.Next(0, gif.Length);

            var embed = new EmbedBuilder() { Color = color };
            embed.WithDescription(sender + " leckt " + receiver + " ab");
            embed.WithImageUrl(gif[i]);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("kiss")]
        [Summary("Küsse deinen Liebling")]
        public async Task Kiss(string receiver)
        {
            string sender = "<@" + Context.User.Id + ">";
            string[] gif =
            {
                "https://img1.ak.crunchyroll.com/i/spire2/f6188c69e8c4486ba2a6b3acb0398f0b1463459407_full.gif",
                "https://i.imgur.com/sGVgr74.gif",
                "https://i.pinimg.com/originals/4e/92/1c/4e921c6c5f2e0961d394773911c83dd8.gif"
            };

            Random rnd = new Random();
            int i = rnd.Next(0, gif.Length);

            var embed = new EmbedBuilder() { Color = color };
            embed.WithDescription(sender + " leckt " + receiver + " ab");
            embed.WithImageUrl(gif[i]);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        // sad gif: https://steamuserimages-a.akamaihd.net/ugc/951845529592135299/A6EC5F3B583226460A91A825FA9363A8A3C1BD9F/
        // TODO: cuddle
        // TODO: cry
        // TODO: slap
        // TODO: sorry (apologize)
        #endregion fun
    }
}
