using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using ServerPlugins.Game.Components;
using Utils;
using Utils.Packets;

namespace ServerPlugins.Game
{
    public class Player : Entity
    {
        public readonly IClient Client;

        readonly Dictionary<ushort, Message> _messages = new Dictionary<ushort, Message>();
        readonly Dictionary<ushort, Action<Message>> _handlers = new Dictionary<ushort, Action<Message>>();

        private NavigationComponent agent;
        
        private bool destinationChanged;

        public Player(IClient client)
        {
            Client = client;
            Client.MessageReceived += OnMessageFromPlayer;
        }

        private void OnMessageFromPlayer(object sender, MessageReceivedEventArgs e)
        {
            var msg = e.GetMessage();
            if (msg != null && _handlers.ContainsKey(msg.Tag))
            {
                lock (_messages)
                {
                    //Overwrite existing message of the same tag
                    _messages[msg.Tag] = msg;
                }
            }
        }

        public override void Start()
        {
            base.Start();
            agent = GetComponent<NavigationComponent>();
            _handlers.Add(MessageTags.NavigateTo, HandleNavigateTo);
        }

        private void HandleNavigateTo(Message message)
        {
            var data = message.Deserialize<NavigateToPacket>();
            if (data != null)
            {
                agent.Destination = data.Destination;
                agent.StoppingDistance = data.StoppingDistance;
                agent.IsDirty = true;
            }
        }

        public override void Update()
        {
            lock (_messages)
            {
                foreach (var tag in _messages.Keys)
                {
                    _handlers[tag].Invoke(_messages[tag]);
                }
                _messages.Clear();
            }

            switch (State)
            {
                case EntityState.Idle:
                    UpdateIdle();
                    break;
                case EntityState.Moving:
                    UpdateMoving();
                    break;
                case EntityState.Casting:
                    UpdateCasting();
                    break;
                case EntityState.Dead:
                    UpdateDead();
                    break;
                default:
                    Game.Log("Entity " + ID + " in unknown state", LogType.Warning);
                    break;

            }
            base.Update();
        }



        private void UpdateIdle()
        {
            if (Health == 0)
            {
                Die();
                State = EntityState.Dead;
            }
            if (agent.IsDirty)
            {
                //TODO: Validate destination
                agent.Navigate();
                agent.IsDirty = false;
                State = EntityState.Moving;
            }
        }

        private void UpdateMoving()
        {
        }
        private void UpdateCasting()
        {

        }

        private void UpdateDead()
        {

        }

        void Die()
        {
            agent.Reset();
            Target = null;
        }
    }
}