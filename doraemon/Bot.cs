using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doraemon
{
    public class Bot
    {
        private string orientation;
        private int[] position;
        public Bot()
        {

        }
        public int[] getPosition()
        {
            return this.position;
        }

        public string getOrientation()
        {
            return this.orientation;
        }

        public void setOrientation()
        {
            // Set the orientation to any of these ['front', 'left', 'right', 'back']
        }

        protected void traverse()
        {
            // Move the bot in forward direction
        }

        protected void turnRight()
        {
            // Turn the bot right
        }

        protected void turnLeft()
        {
            // Turn the bot left
        }

        protected void align()
        {
            // Aligns the bot facing back to Kinect
        }

        protected void stop()
        {
            // Stops the bot
        }
    }
}
