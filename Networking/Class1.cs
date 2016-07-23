using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{

    public class Game
    {
        private Player plr;
        private Enemy enemy;

        private NetworkWizardClient client;

        void Update()
        {

            // grab input and store in ib

            // client.onTick

            //plr.Position += plr.Veclocity;
            //enemy.Position += enemy.Veclocity;

            // un networked updates happen afterwards. 

            // draw maybe this threaded... 

        }

        void Draw()
        {
            
        }
    }

    // Tickable
    // NetworkedTickable

    interface ITickable
    {
        void Tick(object inputDetails);
    }

    interface INetworkedTickable 
    {
        void Tick(object inputDetails);
        void SetState(StateObject state);
    }


    public interface StateHandler
    {
        void ApplyInput(object inputCode);
        void SetState(object stateCode);


    }

    public class Player
    {
        public int Position { get; set; }
        public int Veclocity { get; set; }
    }

    public class Enemy
    {
        public int Position { get; set; }
        public int Veclocity { get; set; }
    }

    public class EnemyWand : StateHandler
    {
        private Enemy e;

        public void ApplyInput(object inputCode) // read as "run simulation for a tick" with optional input
        { 
            // I dont care about this.
        }

        public void SetState(object stateCode)
        {
            
        }
    }

    public class PlayerWand : StateHandler
    {

        private Player _plr;

        public void ApplyInput(object inputCode)
        {
            _plr.Position += 1; // whatever came from input. 
        }

        public void SetState(object stateCode)
        {
            _plr.Position = 3; // todo from status code
        }

    }

    public class StateObject
    {
        public static  StateObject parse(string str)
        {
            return null;
        }

        public static string serialize(StateObject state)
        {
            return "";
        }
    }

    public class NetworkWizardClient
    {
        private List<StateHandler> _wands;
        private InputBuffer _ib;


        void Configure()
        {
            // set up socket
        }

        // has sockets. 
        void OnSocketDump(StateObject dump)
        {
            // parse it!
            // player x
            // player y
            // whatever other information
            var parsed = dump;

            _wands.ForEach(w => w.SetState(parsed));

            // now apply the inputs the recent inputs
            _ib.Ack(0); // parsed.lastInput todo 

            // todo remember that durring OnTick() call, the sim will be replayed to reconcile input. 

        }

        void OnTick()
        {
            // get latest input
            var input = _ib.GetUnappliedInputs(); 

            // apply to the simulation (_wands)
            _wands.ForEach(w => w.ApplyInput(input));

        }

        public void SendInput(object input)
        {
            // gets hucked onto the socket
        }

        
    }


    public abstract class InputBuffer
    {
        private NetworkWizardClient _host;
        public void AddInput(object input)
        {
            // todo impl
            // going to send to the wizardServerHost
            _host.SendInput(input);
        }

        public void Ack(int seqNumber)
        {
            
        }

        public List<object> GetUnacked()
        {
            return new List<object>(); // todo impl
        }

        public abstract List<object> GetUnappliedInputs();

    }
}
