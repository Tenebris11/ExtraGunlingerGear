﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExtraGunGear.NPCs.Bosses.SunGod
{
    [AutoloadBossHead]
    public class SunGod : ModNPC
    {
        private Player player;
        private float speed;
        private float rotationCount;
        private bool changingPhase;
        private int bossPhase;
        private bool canHit;


        private bool closeAttack;

        private float DecisionCounter
        {
            get { return npc.ai[0]; }
            set { npc.ai[0] = value; }
        }

        private float AttackRotation
        {
            get { return npc.ai[1]; }
            set { npc.ai[1] = value; }
        }

        private float AttackCooldown
        {
            get { return npc.ai[2]; }
            set { npc.ai[2] = value; }
        }

        private float PhaseCooldown
        {
            get { return npc.ai[3]; }
            set { npc.ai[3] = value; }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sun God");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.rotation = 0f;
            npc.lifeMax = 1000; //200000;
            npc.damage = 20;
            npc.defense = 50;
            npc.knockBackResist = 0f;
            npc.width = 150;
            npc.height = 150;
            npc.visualOffset = new Vector2(0, 50f);
            npc.value = 100000;
            npc.npcSlots = 3f;
            npc.boss = true;
            npc.lavaImmune = true;
            npc.buffImmune[BuffID.OnFire] = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit30;
            npc.DeathSound = SoundID.NPCDeath33;
            music = MusicID.Boss1;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)((float)npc.lifeMax * 0.75f * bossLifeScale);
            npc.damage = (int)((float)npc.damage * 0.85f);
        }

        public override void AI()
        {
            if (Main.netMode != 1)
            {
                GetTarget();
                Despawn();
                canHit = true;

                PhaseControl();
                if (!changingPhase)
                {
                    MoveToTarget(player.Center, new Vector2(0, -200f), 6f);
                    AttackControl();
                }

                DecisionCounter += 1f;
                if (DecisionCounter > 20f)
                {
                    DecisionCounter = 0f;
                }
                npc.netUpdate = true;
            }
        }
        

        private void GetTarget()
        {
            player = Main.player[npc.target];
        }

        private int PhaseControl()
        {
            if (npc.life <= (float)npc.lifeMax * (2f / 3f))
            {
                if (PhaseCooldown < 200f) ChangePhase(1);
                else if (npc.life <= (float)npc.lifeMax * (1f / 3f))
                {
                    if (PhaseCooldown < 400f) ChangePhase(2);
                }
            }
            else
            {
                bossPhase = 0;
            }
            return bossPhase;
        }

        private void ChangePhase(int phase)
        {
            PhaseCooldown += 1f;
            if (Main.netMode != 1)
            {
                if (PhaseCooldown % 200 == 0)
                {
                    canHit = true;
                    changingPhase = false;
                    npc.immortal = false;
                    bossPhase = phase;
                    PhaseFlames();
                }
                else
                {
                    canHit = false;
                    changingPhase = true;
                    npc.immortal = true;
                }

                float inputValue = (PhaseCooldown % 200f) / 2f;
                //float dampingFunc = (float)(Math.Pow(Math.E, -0.03 * inputValue) * 50.0 * Math.Cos(-0.8 * inputValue));
                //MoveToTarget(new Vector2(npc.position.X + 25f * (float)Math.Sin((Math.PI / 5.0) * inputValue), npc.position.Y), new Vector2(0, 0), 6f);
                MoveToTarget(new Vector2(npc.Center.X + 75f * (float)Main.rand.NextFloat(-1f,1f), npc.Center.Y + 25f * (float)Main.rand.NextFloat(-1f, 1f)), new Vector2(0, 0), 16f, false);

                npc.netUpdate = true;
            }
        }

        private void MoveToTarget(Vector2 target, Vector2 offset, float speed, bool noMove = false)
        {
            if (Main.netMode != 1)
            {
                Vector2 moveTo = target + offset;
                Vector2 move = moveTo - npc.Center;
                float magnitude = MagnitudeOf(move);

                if (magnitude > speed)
                {
                    move *= speed / magnitude;
                }
                float turnResist = 10f;
                move = (npc.velocity * turnResist + move) / (turnResist + 1f);
                magnitude = MagnitudeOf(move);
                if (magnitude > speed)
                {
                    move *= speed / magnitude;
                }

                //if (noMove) move *= 0f;
                npc.velocity = move;
                npc.netUpdate = true;
            }
        }

        private void AttackControl()
        {
            if (Main.netMode != 1)
            {
                double randomAttack = Main.rand.Next(3);
                if (AttackCooldown % 100 == 0)
                {
                    if (bossPhase == 2 && randomAttack == 2) // Randomly decides if attack is used
                    {
                        NapalmRun();
                    }
                    else if (bossPhase >= 1 && randomAttack == 1) // Randomly decides if attack is used
                    {
                        DropBombs();
                    }
                    else // if neither attack is used, then default to flames
                    {
                        ShootFlames();
                    }
                }

                AttackCooldown += 1f;
                if (AttackCooldown > 100f)
                {
                    AttackCooldown = 0f;
                }
                npc.netUpdate = true;
            }
        }

        private void ShootFlames()
        {
            if (Main.netMode != 1)
            {
                int type = mod.ProjectileType("SunGodFlames");
                Vector2 velocity = player.Center - npc.Center;
                float magnitude = MagnitudeOf(velocity);
                if (magnitude > 0)
                {
                    velocity *= 12f / magnitude; // Normalizes vector and multiplies by value
                }
                else
                {
                    velocity = new Vector2(0f, 5f);
                }

                float ratio = velocity.X / (-1f * velocity.Y);
                float normalMagn = (float)Math.Sqrt(1.0 + Math.Pow(ratio, 2.0));

                Vector2 flameOffset = new Vector2(0, 0)
                {
                    X = 1f / normalMagn,
                    Y = ratio / normalMagn
                };

                for (int i = -50; i <= 50; i += 10)
                {
                    float velocityRandomFactor = Main.rand.NextFloat(0.75f, 1.25f);
                    Vector2 offsetVelocity = velocity * velocityRandomFactor;
                    //for (int j = 0; j <= 5; ++j)
                    {
                        Vector2 temp = new Vector2(flameOffset.X, flameOffset.Y);
                        temp *= i;
                        int damage = (npc.damage) / 2;
                        Projectile.NewProjectile(new Vector2(npc.Center.X + temp.X, npc.Center.Y + temp.Y), offsetVelocity, type, damage, 0f);
                    }
                }
                AttackCooldown = 0;
                npc.netUpdate = true;
            }
        }

        private void DropBombs()
        {

        }

        private void NapalmRun()
        {

        }

        private void PhaseFlames()
        {
            Vector2 velocity = player.Center - npc.Center;
            float magnitude = MagnitudeOf(velocity);
            if (magnitude > 0)
            {
                velocity *= 5f / magnitude; // Normalizes vector and multiplies by value
            }
            else
            {
                velocity = new Vector2(0f, 5f);
            }

            float ratio = velocity.X / (-1f * velocity.Y);
            float normalMagn = (float)Math.Sqrt(1.0 + Math.Pow(ratio, 2.0));

            Vector2 flameOffset = new Vector2(0, 0)
            {
                X = 1f / normalMagn,
                Y = ratio / normalMagn
            };

            int segments = 30;
            for (int i = 0; i <= segments; ++i)
            {
                float velocityRandomFactor = Main.rand.NextFloat(0.75f, 1.25f);
                Vector2 offsetVelocity = velocity * velocityRandomFactor;

                Vector2 temp = new Vector2(flameOffset.X, flameOffset.Y);
                temp *= i;
                Projectile.NewProjectile(new Vector2(npc.Center.X + temp.X, npc.Center.Y + temp.Y), offsetVelocity, mod.ProjectileType("SunGodFlames"), npc.damage, 0f);
                velocity = velocity.RotatedBy((2f * Math.PI) / (float)segments); // divide the circle into segments so each value of i is equally spaced
            }
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            if (Main.expertMode)
            {
                player.AddBuff(BuffID.OnFire, 600, true);
            }
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            if (!canHit)
            {
                damage = 0;
                canHit = true;
                Main.PlaySound(npc.HitSound, npc.position);
                return false;
            }
            return true;
        }

        private void Despawn()
        {
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if(!player.active || player.dead)
                {
                    npc.velocity = new Vector2(0f, -10f);
                    if(npc.timeLeft > 10)
                    {
                        npc.timeLeft = 10;
                    }
                    return;
                }
            }
        }

        private float MagnitudeOf(Vector2 vector2)
        {
            return (float)Math.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y);
        }
        
        /*public override void FindFrame(int frameHeight)
        {
            npc.frameCounter += 1;
            npc.frameCounter %= 20;
            int frame = (int)(npc.frameCounter / 2.0);
            if(frame >= Main.npcFrameCount[npc.type])
            {
                frame = 0;
            }
            npc.frame.Y = frame * frameHeight;
        }*/

        public override void NPCLoot()
        {
            base.NPCLoot();
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        }

        public float Rotate(float angle)
        {
            rotationCount += angle;
            return (float)Math.Asin((double)Math.Sin(rotationCount)) * 2;
        }

        private void DrawCorona(int phase, SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D corona;
            switch (phase)
            {
                case 0:
                    corona = mod.GetTexture("NPCs/Bosses/SunGod/SunGodCorona");
                    break;
                case 1:
                    corona = mod.GetTexture("NPCs/Bosses/SunGod/SunGodCoronaYellow");
                    break;
                case 2:
                    corona = mod.GetTexture("NPCs/Bosses/SunGod/SunGodCoronaBlue");
                    break;
                default:
                    corona = mod.GetTexture("NPCs/Bosses/SunGod/SunGodCorona");
                    break;
            }
            spriteBatch.Draw
            (
                corona,
                new Vector2
                (
                    npc.position.X - Main.screenPosition.X + npc.width * 0.5f,
                    npc.position.Y - Main.screenPosition.Y + npc.height * 0.5f
                ),
                new Rectangle(0, 0, corona.Width, corona.Height),
                drawColor,
                Rotate(0.01f),
                corona.Size() * 0.5f,
                npc.scale,
                SpriteEffects.None,
                0f
            );
            spriteBatch.Draw
            (
                corona,
                new Vector2
                (
                    npc.position.X - Main.screenPosition.X + npc.width * 0.5f,
                    npc.position.Y - Main.screenPosition.Y + npc.height * 0.5f
                ),
                new Rectangle(0, 0, corona.Width, corona.Height),
                drawColor,
                Rotate(0.01f) * -1f,
                corona.Size() * 0.5f,
                npc.scale,
                SpriteEffects.None,
                0f
            );
        }
        private void DrawBody(int phase, SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D body;
            switch (phase)
            {
                case 0:
                    body = mod.GetTexture("NPCs/Bosses/SunGod/SunGodBodyRed");
                    break;
                case 1:
                    body = mod.GetTexture("NPCs/Bosses/SunGod/SunGodBodyYellow");
                    break;
                case 2:
                    body = mod.GetTexture("NPCs/Bosses/SunGod/SunGodBodyBlue");
                    break;
                default:
                    body = mod.GetTexture("NPCs/Bosses/SunGod/SunGodBodyYellow");
                    break;
            }

            spriteBatch.Draw
            (
                body,
                new Vector2
                (
                    npc.position.X - Main.screenPosition.X + npc.width * 0.5f,
                    npc.position.Y - Main.screenPosition.Y + npc.height * 0.5f
                ),
                new Rectangle(0, 0, body.Width, body.Height),
                drawColor,
                npc.rotation,
                body.Size() * 0.5f,
                npc.scale,
                SpriteEffects.None,
                0f
            );
        }
        private void DrawFace(int phase, SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D face;
            if (!changingPhase) face = mod.GetTexture("NPCs/Bosses/SunGod/SunGodFace");
            else face = mod.GetTexture("NPCs/Bosses/SunGod/SunGodCrossEyes");
            spriteBatch.Draw
            (
                face,
                new Vector2
                (
                    npc.position.X - Main.screenPosition.X + npc.width * 0.5f,
                    npc.position.Y - Main.screenPosition.Y + npc.height * 0.5f
                ),
                new Rectangle(0, 0, face.Width, face.Height),
                drawColor,
                npc.rotation,
                face.Size() * 0.5f,
                npc.scale,
                SpriteEffects.None,
                0f
            );

            Texture2D smile;
            switch (phase)
            {
                case 0:
                    smile = mod.GetTexture("NPCs/Bosses/SunGod/SunGodSmile");
                    break;
                case 1:
                    smile = mod.GetTexture("NPCs/Bosses/SunGod/SunGodDisgust");
                    break;
                case 2:
                    smile = mod.GetTexture("NPCs/Bosses/SunGod/SunGodGrimace");
                    break;
                default:
                    smile = mod.GetTexture("NPCs/Bosses/SunGod/SunGodSmile");
                    break;
            }
            if (!changingPhase)
            spriteBatch.Draw
            (
                smile,
                new Vector2
                (
                    npc.position.X - Main.screenPosition.X + npc.width * 0.5f,
                    npc.position.Y - Main.screenPosition.Y + npc.height * 0.5f
                ),
                new Rectangle(0, 0, smile.Width, smile.Height),
                drawColor,
                npc.rotation,
                smile.Size() * 0.5f,
                npc.scale,
                SpriteEffects.None,
                0f
            );
        }

        // Custom rendering //
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            DrawCorona(bossPhase, spriteBatch, Color.White);
            DrawBody(bossPhase, spriteBatch, Color.White);

            DrawFace(bossPhase, spriteBatch, Color.White);
            return false;
        }
    }
}