using System.Linq;

using Vigilance.API;
using Vigilance.API.Enums;

using Vigilance.Commands;

using Vigilance.External.Extensions;
using Vigilance.External.Extensions.Rpc;

using PlayableScps;

namespace Vigilance.External.Commands
{
    public class CommandPlayerAction : ICommandHandler
    {
        public string Command => "playeraction";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: playeraction <action>\nRun \"playeraction list\" to get a list of all actions.";

            if (!CommandSelectPlayer.SelectedPlayers.TryGetValue(sender, out Player player))
                return "You must select a player first. Do so by using the \"selectplayer\" command.";

            if (player == null)
                return "Target Player not found - perhaps he already left the server.";

            if (args[0] == "addItem")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction additem <itemType>\"";

                ItemType item = args[1].GetItem();

                player.AddItem(item);

                return $"Added a {item} to {player.Nick}'s inventory.";
            }

            if (args[0] == "ahp")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction ahp <amount>\"";

                if (!float.TryParse(args[1], out float amount))
                    return "\"amount\" must be a valid integer.";

                player.ArtificialHealth = amount;

                return $"{player.Nick}'s artificial health was set to {amount}";
            }

            if (args[0] == "maxAhp")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction maxAhp <amount>\"";

                if (!int.TryParse(args[1], out int amount))
                    return "\"amount\" must be a valid integer.";

                player.MaxArtificalHealth = amount;

                return $"{player.Nick}'s max artificial health was set to {amount}";
            }

            if (args[0] == "closeRa")
            {
                player.CloseRemoteAdmin();
                return $"Closed {player.Nick}'s remote admin panel.";
            }

            if (args[0] == "openRa")
            {
                player.OpenRemoteAdmin();
                return $"Opened {player.Nick}'s remote admin panel.";
            }

            if (args[0] == "clearinv")
            {
                player.ClearInventory();
                return $"Cleared {player.Nick}'s inventory.";
            }

            if (args[0] == "consoleMessage")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction consoleMessage <message>\"";

                string message = args.Skip(1).ToArray().Combine();

                player.ConsoleMessage(message);

                return $"Sent a message to {player.Nick}'s console.";
            }

            if (args[0] == "exp")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction exp <amount>\"";

                if (!float.TryParse(args[1], out float amount))
                    return "\"exp\" must be a valid integer.";

                player.Experience = amount;

                return $"{player.Nick}'s experience was set to {amount}";
            }

            if (args[0] == "lvl")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction lvl <amount>\"";

                if (!byte.TryParse(args[1], out byte level))
                    return "\"lvl\" must be a valid integer.";

                player.Level = level;

                return $"{player.Nick}'s level was set to {level}";
            }

            if (args[0] == "energy")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction energy <amount>\"";

                if (!float.TryParse(args[1], out float amount))
                    return "\"amount\" must be a valid integer.";

                player.Energy = amount;

                return $"{player.Nick}'s energy was set to {amount}";
            }

            if (args[0] == "hp")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction hp <amount>\"";

                if (!int.TryParse(args[1], out int amount))
                    return "\"hp\" must be a valid integer.";

                player.Health = amount;

                return $"{player.Nick}'s health was set to {amount}";
            }

            if (args[0] == "maxHp")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction maxHp <amount>\"";

                if (!int.TryParse(args[1], out int amount))
                    return "\"maxHp\" must be a valid integer.";

                player.MaxHealth = amount;

                return $"{player.Nick}'s max health was set to {amount}";
            }

            if (args[0] == "maxEnergy")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction maxEnergy <amount>\"";

                if (!float.TryParse(args[1], out float amount))
                    return "\"maxEnegery\" must be a valid integer.";

                player.MaxEnergy = amount;

                return $"{player.Nick}'s max energy was set to {amount}";
            }

            if (args[0] == "enrage")
            {
                if (player.Role != RoleType.Scp096)
                    return $"{player.Nick} is not Scp096.";
                else
                {
                    Scp096 scp = player.CurrentScp as Scp096;
                    scp.Enrage();

                    return $"Enraged {player.Nick}";
                }    
            }

            if (args[0] == "calm")
            {
                if (player.Role != RoleType.Scp096)
                    return $"{player.Nick} is not Scp096.";
                else
                {
                    Scp096 scp = player.CurrentScp as Scp096;
                    scp.EndEnrage();

                    return $"Calmed {player.Nick}";
                }
            }

            if (args[0] == "kill")
            {
                player.Kill();
                return $"Killed {player.Nick}";
            }

            if (args[0] == "rankColor")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction rankColor <color>\"";

                player.ReferenceHub.serverRoles.SetColor(args[1]);

                return $"{player.Nick}'s rank color was set to {args[1]}";
            }

            if (args[0] == "rankText")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction rankText <color>\"";
                string text = args.Skip(1).ToArray().Combine();

                player.ReferenceHub.serverRoles.SetText(text);

                return $"{player.Nick}'s rank color was set to {text}";
            }

            if (args[0] == "createPortal")
            {
                if (player.Role != RoleType.Scp106)
                    return $"{player.Nick} is not Scp106.";
                else
                {
                    player.CreatePortal();
                }
            }

            if (args[0] == "usePortal")
            {
                if (player.Role != RoleType.Scp106)
                    return $"{player.Nick} is not Scp106.";
                else
                {
                    player.UsePortal();
                }
            }

            if (args[0] == "resetStamina")
            {
                player.ResetStamina();
                return $"Stamina of {player.Nick} was reset.";
            }

            if (args[0] == "raMessage")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction raMessage <message>\"";

                string text = args.Skip(1).ToArray().Combine();

                player.SendRemoteAdminMessage(text);

                return $"Sent a message to {player.Nick}'s RA.";
            }

            if (args[0] == "shake")
            {
                player.Shake();
                return $"Shaked {player.Nick}'s camera.";
            }

            if (args[0] == "changeRole")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction changeRole <role>\"";

                RoleType role = args[1].GetRole();

                player.ChangeAppearance(role);

                return $"Succesfully changed appearance of {player.Nick} to {role}.";
            }

            if (args[0] == "setRole")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction setRole <role>\"";

                RoleType role = args[1].GetRole();

                player.SetRole(role, true, false);

                return $"Succesfully changed role of {player.Nick} to {role}.";
            }

            if (args[0] == "list")
                return "setRole, changeRole, shake, raMessage, resetStamina, usePortal, createPortal, rankText, rankColor, kill, calm, enrage, maxEnergy, energy, lvl, exp, hp, maxHp, " +
                    "ahp, maxAhp, consoleMessage, clearinv, openRa, closeRa, addItem";

            if (args[0] == "rpc")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"playeraction rpc <rpcName>\"\nUse \"playeraction rpc list\" to get a list of all available RPCs\nmRPC (Remote Procedure Call) is used for sending call requests to specific players, so for example you can play an ambient sound for one player only.";

                if (args[1] == "zombieAttack")
                {
                    player.Rpc0492DamageAnimation();
                    return "Done.";
                }

                if (args[1] == "escapeMessage")
                {
                    player.RpcEscapeMessage(false, true);
                    return "Done.";
                }

                if (args[1] == "fallDamageSound")
                {
                    player.RpcFalldamageSound();
                    return "Done.";
                }

                if (args[1] == "fastRestart")
                {
                    player.RpcFastRestart();
                    return "Done.";
                }

                if (args[1] == "flickerLights")
                {
                    player.RpcFlickerLights(10f);
                    return "Done.";
                }

                if (args[1] == "hidHitmarker")
                {
                    player.RpcHidHitmarker(false);
                    return "Done.";
                }

                if (args[1] == "intercomSound")
                {
                    player.RpcIntercomSound(true, player.PlayerId);
                    return "Done.";
                }

                if (args[1] == "liftMusic")
                {
                    player.RpcLiftMusic();
                    return "Done.";
                }

                if (args[1] == "playSound")
                {
                    if (args.Length < 3)
                        return "Missing arguments!\nUsage: \"playeraction rpc playSound <soundId>";

                    if (!int.TryParse(args[2], out int id))
                        return "SoundID must be a valid integer.";

                    player.RpcPlaySound(id);

                    return "Done.";
                }

                if (args[1] == "targetInfo")
                {
                    if (args.Length < 4)
                        return "Missing arguments!\nUsage: \"playeraction rpc targetInfo <target> <info>";

                    Player target = args[2].GetPlayer();

                    if (target == null)
                        return "Target not found.";

                    string info = args.SkipWords(3);

                    player.SetPlayerInfoForTargetOnly(target, info);

                    return "Done.";
                }

                if (args[1] == "list")
                    return "zombieAttack, targetInfo, playSound, liftMusic, intercomSound, hidHitmarker, flickerLights, fastRestart, fallDamageSound, escapeMessage";
            }

            return "Missing arguments!\nUsage: playeraction <action>\nRun \"playeraction actions\" to get a list of all actions.";
        }
    }
}