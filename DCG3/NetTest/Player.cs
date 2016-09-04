using DCG.Framework.Net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG3.NetTest
{
    class PlayerStateHandler : INetStateHandler<SomeNetState>
    {
        public Player Player { get; set; }

        public void ApplyInput(InputCollection inputs)
        {
            Player.Tick(inputs);
        }

        public void SetState(SomeNetState state)
        {
            Player.Position = state.PlayerPosition;
            Player.Velocity = state.PlayerVelocity;
        }
    }

    class Player 
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public void Tick(InputCollection ic)
        {

            var acc = ic.Get<MoveInput>();
            Velocity += acc.TypedValue;
            //Velocity += ic.Get<MoveInput>()?.Value;

            Velocity += -Velocity * .9f;    // friction
            Velocity += Position;           // additive motion

        }

    }
}
