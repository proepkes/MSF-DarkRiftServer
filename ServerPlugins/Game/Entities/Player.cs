using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game.Entities
{
    public class Player : Entity
    {
        public readonly IClient Client;

        readonly Dictionary<ushort, Message> _messages = new Dictionary<ushort, Message>();
        readonly Dictionary<ushort, Action<Message>> _handlers = new Dictionary<ushort, Action<Message>>();

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
            _handlers.Add(MessageTags.NavigateTo, HandleNavigateTo);
        }

        private void HandleNavigateTo(Message message)
        {
            var data = message.Deserialize<NavigateToPacket>();
            if (data != null)
            {
                agent.StoppingDistance = data.StoppingDistance;
                agent.Navigate(TundraVector3.Create(data.Destination.X, data.Destination.Y, data.Destination.Z));
            }
        }

        public override void Update(float delta)
        {
            base.Update(delta);
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
        }



        private void UpdateIdle()
        {
            if (Health == 0)
            {
                Die();
                State = EntityState.Dead;
            }
            else if (agent.HasPath)
            {
                State = EntityState.Moving;
            }
        }

        private void UpdateMoving()
        {
            if (Health == 0)
            {
                Die();
                State = EntityState.Dead;
            }
            else if (agent.HasPath)
            {
                State = EntityState.Moving;
            }
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
            SetTarget(null);
        }
    }
}