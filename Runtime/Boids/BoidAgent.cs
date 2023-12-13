using System.Collections.Generic;
using UnityEngine;


namespace CommonBase
{
    public class BoidAgent : MonoBehaviour
    {
        private float visualRange = 40;
        protected float protectedRange = 8;
        private float centeringfactor = .0005f;
        private float matchingfactor = .05f;
        private float avoidfactor = .05f;

        private float turnFactor = .5f;
        private float maxSpeed = 12f;
        private float minSpeed = 3f;
        private float maxbias = .01f;
        private float bias_increment = .00004f;
        private float biasval = .001f;

        public Vector2 position;
        public Vector2 velocity;

        public void InitPos(Vector2 pos)
        {
            this.position = pos;
        }

        //遮挡剔除
        //能否合并多个计时器到一个计时器中
        public void Tick(List<BoidAgent> all, float dt)
        {
            //separation
            float close_dx = 0;
            float close_dy = 0;




            foreach (var other in all)
            {
                if (other == this) continue;
                if (Vector2.Distance(other.position, this.position) < protectedRange)
                {
                    close_dx += (this.position - other.position).x;
                    close_dy += (this.position - other.position).y;
                }
            }

            this.velocity.x += close_dx * avoidfactor;
            this.velocity.y += close_dy * avoidfactor;


            //alignment
            float xv_avg = 0;
            float yv_avg = 0;
            int neighboring_boids = 0;

            foreach (var other in all)
            {
                if (other == this) continue;
                if (Vector2.Distance(other.position, this.position) < visualRange)
                {
                    xv_avg += other.velocity.x;
                    yv_avg += other.velocity.y;
                    neighboring_boids++;
                }
            }

            xv_avg = xv_avg / neighboring_boids;
            yv_avg = yv_avg / neighboring_boids;

            this.velocity.x += (xv_avg - this.velocity.x) * matchingfactor;
            this.velocity.y += (yv_avg - this.velocity.y) * matchingfactor;

            //cohesion
            float xpos_avg = 0;
            float ypos_avg = 0;
            int neighboring_boids2 = 0;

            foreach (var other in all)
            {
                if (other == this) continue;
                if (Vector2.Distance(other.position, this.position) < visualRange)
                {
                    xpos_avg += other.position.x;
                    ypos_avg += other.position.y;
                    neighboring_boids2++;
                }

            }


            xpos_avg = xpos_avg / neighboring_boids2;
            ypos_avg = ypos_avg / neighboring_boids2;

            this.velocity.x += (xpos_avg - this.position.x) * centeringfactor;
            this.velocity.y += (ypos_avg - this.position.y) * centeringfactor;

            //edges
            if (this.position.x < -50)
            {
                this.velocity.x += turnFactor;
            }
            if (this.position.x > 50)
            {
                this.velocity.x -= turnFactor;
            }
            if (this.position.y < -50)
            {
                this.velocity.y += turnFactor;
            }
            if (this.position.y > 50)
            {
                this.velocity.y -= turnFactor;
            }


            //limit speed
            this.velocity = this.velocity.magnitude > maxSpeed ? maxSpeed / this.velocity.magnitude * this.velocity : this.velocity;

            //update pos
            this.position += this.velocity * dt;
            this.transform.position = this.position;
        }
    }

}
