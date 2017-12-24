using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;

namespace ExtraGunGear.Projectiles
{
    public class VortexARProj : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 72;
            projectile.height = 34;
            projectile.scale = 1f;
            projectile.aiStyle = 0;
            projectile.timeLeft = 999999;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.ranged = true;
            projectile.penetrate = -1;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vortex Pulse Rifle");
        }

        public override void AI()
        {
            //Settings for updating on net
            Vector2 vector22 = Main.player[projectile.owner].RotatedRelativePoint(Main.player[projectile.owner].MountedCenter, true);
            if (Main.myPlayer == projectile.owner)
            {
                if (Main.player[projectile.owner].channel)
                {
                    float num263 = Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].shootSpeed * projectile.scale;
                    Vector2 vector23 = vector22;
                    float num264 = (float)Main.mouseX + Main.screenPosition.X - vector23.X;
                    float num265 = (float)Main.mouseY + Main.screenPosition.Y - vector23.Y;
                    if (Main.player[projectile.owner].gravDir == -1f)
                    {
                        num265 = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector23.Y;
                    }
                    float num266 = (float)Math.Sqrt((double)(num264 * num264 + num265 * num265));
                    num266 = (float)Math.Sqrt((double)(num264 * num264 + num265 * num265));
                    num266 = num263 / num266;
                    num264 *= num266;
                    num265 *= num266;
                    if (num264 != projectile.velocity.X || num265 != projectile.velocity.Y)
                    {
                        projectile.netUpdate = true;
                    }
                    projectile.velocity.X = num264;
                    projectile.velocity.Y = num265;
                }
                else
                {
                    projectile.Kill();
                }
            }

            //Setting the position and direction
            if (projectile.velocity.X > 0f)
            {
                Main.player[projectile.owner].ChangeDir(1);
            }
            else
            {
                if (projectile.velocity.X < 0f)
                {
                    Main.player[projectile.owner].ChangeDir(-1);
                }
            }
            projectile.spriteDirection = projectile.direction;
            Main.player[projectile.owner].ChangeDir(projectile.direction);
            Main.player[projectile.owner].heldProj = projectile.whoAmI;
            projectile.position.X = vector22.X - (float)(projectile.width / 2);
            projectile.position.Y = vector22.Y - (float)(projectile.height / 2);
            projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 2.355f;

            //Rotation of Projectile and Item Use
            if (projectile.spriteDirection == -1)
            {
                projectile.rotation -= 1.57f;
            }
            if (Main.player[projectile.owner].direction == 1)
            {
                Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));
            }
            else
            {
                Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));
            }

            //Reset how long the projectile lives
            Main.player[projectile.owner].itemTime = 10;
            Main.player[projectile.owner].itemAnimation = 10;
            projectile.timeLeft = 999999;
        }
    }
}