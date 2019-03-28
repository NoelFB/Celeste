using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Pico8
{
    /// <summary>
    /// Attempting to Copy the Celeste Classic line-for-line here.
    /// Obviously some differences due to converting from LUA to C#
    /// 
    /// This is not how I would recommend implementing a game like this from scratch in C#!
    /// It's simply trying to be 1-1 with the LUA version
    /// </summary>
    public class Classic
    {        
        public Emulator E;

        // ~ celeste ~
        // matt thorson + noel berry

        #region "global" variables

        private Point room;
        private List<ClassicObject> objects;
        public int freeze;
        private int shake;
        private bool will_restart;
        private int delay_restart;
        private HashSet<int> got_fruit;
        private bool has_dashed;
        private int sfx_timer;
        private bool has_key;
        private bool pause_player;
        private bool flash_bg;
        private int music_timer;
        private bool new_bg;

        private int k_left = 0;
        private int k_right = 1;
        private int k_up = 2;
        private int k_down = 3;
        private int k_jump = 4;
        private int k_dash = 5;

        private int frames;
        private int seconds;
        private int minutes;
        private int deaths;
        private int max_djump;
        private bool start_game;
        private int start_game_flash;

        #endregion

        #region effects

        private class Cloud
        {
            public float x;
            public float y;
            public float spd;
            public float w;
        }
        private List<Cloud> clouds;

        private class Particle
        {
            public float x;
            public float y;
            public int s;
            public float spd;
            public float off;
            public int c;
        }
        private List<Particle> particles;

        private class DeadParticle
        {
            public float x;
            public float y;
            public int t;
            public Vector2 spd;
        }
        private List<DeadParticle> dead_particles;

        #endregion

        #region entry point

        public void Init(Emulator emulator)
        {
            E = emulator;

            room = new Point(0, 0);
            objects = new List<ClassicObject>();
            freeze = 0;
            will_restart = false;
            delay_restart = 0;
            got_fruit = new HashSet<int>();
            has_dashed = false;
            sfx_timer = 0;
            has_key = false;
            pause_player = false;
            flash_bg = false;
            music_timer = 0;
            new_bg = false;

            frames = 0;
            seconds = 0;
            minutes = 0;
            deaths = 0;
            max_djump = 1;
            start_game = false;
            start_game_flash = 0;

            clouds = new List<Cloud>();
            for (int i = 0; i <= 16; i++)
                clouds.Add(new Cloud()
                {
                    x = E.rnd(128),
                    y = E.rnd(128),
                    spd = 1 + E.rnd(4),
                    w = 32 + E.rnd(32)
                });

            particles = new List<Particle>();
            for (int i = 0; i <= 32; i++)
                particles.Add(new Particle()
                {
                    x = E.rnd(128),
                    y = E.rnd(128),
                    s = 0 + E.flr(E.rnd(5) / 4),
                    spd = 0.25f + E.rnd(5),
                    off = E.rnd(1),
                    c = 6 + E.flr(0.5f + E.rnd(1))
                });

            dead_particles = new List<DeadParticle>();

            title_screen();
        }

        private void title_screen()
        {
            got_fruit = new HashSet<int>();
            frames = 0;
            deaths = 0;
            max_djump = 1;
            start_game = false;
            start_game_flash = 0;
            E.music(40, 0, 7);
            load_room(7, 3);
        }

        private void begin_game()
        {
            frames = 0;
            seconds = 0;
            minutes = 0;
            music_timer = 0;
            start_game = false;
            E.music(0, 0, 7);
            load_room(0, 0);
        }

        private int level_index()
        {
            return room.X % 8 + room.Y * 8;
        }

        private bool is_title()
        {
            return level_index() == 31;
        }

        #endregion

        #region objects

        public class player : ClassicObject
        {
            public bool p_jump = false;
            public bool p_dash = false;
            public int grace = 0;
            public int jbuffer = 0;
            public int djump;
            public int dash_time = 0;
            public int dash_effect_time = 0;
            public Vector2 dash_target = new Vector2(0, 0);
            public Vector2 dash_accel = new Vector2(0, 0);
            public float spr_off = 0;
            public bool was_on_ground;
            public player_hair hair;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                spr = 1;
                djump = g.max_djump;
                hitbox = new Rectangle(1, 3, 6, 5);
            }

            public override void update()
            {
                if (G.pause_player) return;
                var input = E.btn(G.k_right) ? 1 : (E.btn(G.k_left) ? -1 : 0);

                // spikes collide
                if (G.spikes_at(x + hitbox.X, y + hitbox.Y, hitbox.Width, hitbox.Height, spd.X, spd.Y))
                    G.kill_player(this);

                // bottom death
                if (y > 128)
                    G.kill_player(this);

                var on_ground = is_solid(0, 1);
                var on_ice = is_ice(0, 1);

                // smoke particles
                if (on_ground && !was_on_ground)
                    G.init_object(new smoke(), x, y + 4);

                var jump = E.btn(G.k_jump) && !p_jump;
                p_jump = E.btn(G.k_jump);
                if (jump)
                    jbuffer = 4;
                else if (jbuffer > 0)
                    jbuffer--;

                var dash = E.btn(G.k_dash) && !p_dash;
                p_dash = E.btn(G.k_dash);

                if (on_ground)
                {
                    grace = 6;
                    if (djump < G.max_djump)
                    {
                        G.psfx(54);
                        djump = G.max_djump;
                    }
                }
                else if (grace > 0)
                    grace--;

                dash_effect_time--;
                if (dash_time > 0)
                {
                    G.init_object(new smoke(), x, y);
                    dash_time--;
                    spd.X = G.appr(spd.X, dash_target.X, dash_accel.X);
                    spd.Y = G.appr(spd.Y, dash_target.Y, dash_accel.Y);
                }
                else
                {
                    // move
                    var maxrun = 1;
                    var accel = 0.6f;
                    var deccel = 0.15f;

                    if (!on_ground)
                        accel = 0.4f;
                    else if (on_ice)
                    {
                        accel = 0.05f;
                        if (input == (flipX ? -1 : 1)) // this it how it was in the pico-8 cart but is redundant?
                            accel = 0.05f;
                    }

                    if (E.abs(spd.X) > maxrun)
                        spd.X = G.appr(spd.X, E.sign(spd.X) * maxrun, deccel);
                    else
                        spd.X = G.appr(spd.X, input * maxrun, accel);

                    // facing
                    if (spd.X != 0)
                        flipX = (spd.X < 0);

                    // gravity
                    var maxfall = 2f;
                    var gravity = 0.21f;

                    if (E.abs(spd.Y) <= 0.15f)
                        gravity *= 0.5f;

                    // wall slide
                    if (input != 0 && is_solid(input, 0) && !is_ice(input, 0))
                    {
                        maxfall = 0.4f;
                        if (E.rnd(10) < 2)
                            G.init_object(new smoke(), x + input * 6, y);
                    }

                    if (!on_ground)
                        spd.Y = G.appr(spd.Y, maxfall, gravity);

                    // jump
                    if (jbuffer > 0)
                    {
                        if (grace > 0)
                        {
                            // normal jump
                            G.psfx(1);
                            jbuffer = 0;
                            grace = 0;
                            spd.Y = -2;
                            G.init_object(new smoke(), x, y + 4);
                        }
                        else
                        {
                            // wall jump
                            var wall_dir = (is_solid(-3, 0) ? -1 : (is_solid(3, 0) ? 1 : 0));
                            if (wall_dir != 0)
                            {
                                G.psfx(2);
                                jbuffer = 0;
                                spd.Y = -2;
                                spd.X = -wall_dir * (maxrun + 1);
                                if (is_ice(wall_dir * 3, 0))
                                    G.init_object(new smoke(), x + wall_dir * 6, y);
                            }
                        }
                    }

                    // dash
                    var d_full = 5;
                    var d_half = d_full * 0.70710678118f;

                    if (djump > 0 && dash)
                    {
                        G.init_object(new smoke(), x, y);
                        djump --;
                        dash_time = 4;
                        G.has_dashed = true;
                        dash_effect_time = 10;

                        var dash_x_input = E.dashDirectionX(flipX ? -1 : 1);
                        var dash_y_input = E.dashDirectionY(flipX ? -1 : 1);

                        if (dash_x_input != 0 && dash_y_input != 0)
                        {
                            spd.X = dash_x_input * d_half;
                            spd.Y = dash_y_input * d_half;
                        }
                        else if (dash_x_input != 0)
                        {
                            spd.X = dash_x_input * d_full;
                            spd.Y = 0;
                        }
                        else
                        {
                            spd.X = 0;
                            spd.Y = dash_y_input * d_full;
                        }

                        G.psfx(3);
                        G.freeze = 2;
                        G.shake = 6;
                        dash_target.X = 2 * E.sign(spd.X);
                        dash_target.Y = 2 * E.sign(spd.Y);
                        dash_accel.X = 1.5f;
                        dash_accel.Y = 1.5f;

                        if (spd.Y < 0)
                            dash_target.Y *= 0.75f;
                        if (spd.Y != 0)
                            dash_accel.X *= 0.70710678118f;
                        if (spd.X != 0)
                            dash_accel.Y *= 0.70710678118f;
                    }
                    else if (dash && djump <= 0)
                    {
                        G.psfx(9);
                        G.init_object(new smoke(), x, y);
                    }
                }

                // animation
                spr_off += 0.25f;
                if (!on_ground)
                {
                    if (is_solid(input, 0))
                        spr = 5;
                    else
                        spr = 3;
                }
                else if (E.btn(G.k_down))
                    spr = 6;
                else if (E.btn(G.k_up))
                    spr = 7;
                else if (spd.X == 0 || (!E.btn(G.k_left) && !E.btn(G.k_right)))
                    spr = 1;
                else
                    spr = 1 + spr_off % 4;

                // next level
                if (y < -4 && G.level_index() < 30)
                    G.next_room();

                // was on the ground
                was_on_ground = on_ground;
            }
            public override void draw()
            {
                // clamp in screen
                if (x < -1 || x > 121)
                {
                    x = G.clamp(x, -1, 121);
                    spd.X = 0;
                }

                hair.draw_hair(this, flipX ? -1 : 1, djump);
                G.draw_player(this, djump);
            }
        }

        private void psfx(int num)
        {
            if (sfx_timer <= 0)
                E.sfx(num);
        }

        private void draw_player(ClassicObject obj, int djump)
        {
            var spritePush = 0;
            if (djump == 2)
            {
                if (E.flr((frames / 3) % 2) == 0)
                    spritePush = 10 * 16;
                else
                    spritePush = 9 * 16;
            }
            else if (djump == 0)
            {
                spritePush = 8 * 16;
            }
            
            E.spr(obj.spr + spritePush, obj.x, obj.y, 1, 1, obj.flipX, obj.flipY);
        }

        public class player_hair
        {
            private class node
            {
                public float x;
                public float y;
                public float size;
            }

            private node[] hair = new node[5];
            private Emulator E;
            private Classic G;

            public player_hair(ClassicObject obj)
            {
                E = obj.E;
                G = obj.G;
                for (var i = 0; i <= 4; i++)
                    hair[i] = new node() { x = obj.x, y = obj.y, size = E.max(1, E.min(2, 3 - i)) };
            }

            public void draw_hair(ClassicObject obj, int facing, int djump)
            {
                var c = (djump == 1 ? 8 : (djump == 2 ? (7 + E.flr((G.frames / 3) % 2) * 4) : 12));
                var last = new Vector2(obj.x + 4 - facing * 2, obj.y + (E.btn(G.k_down) ? 4 : 3));
                foreach (var h in hair)
                {
                    h.x += (last.X - h.x) / 1.5f;
                    h.y += (last.Y + 0.5f - h.y) / 1.5f;
                    E.circfill(h.x, h.y, h.size, c);
                    last = new Vector2(h.x, h.y);
                }
            }
        }
        
        public class player_spawn : ClassicObject
        {
            private Vector2 target;
            private int state;
            private int delay;
            private player_hair hair;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                spr = 3;
                target = new Vector2(x, y);
                y = 128;
                spd.Y = -4;
                state = 0;
                delay = 0;
                solids = false;
                hair = new player_hair(this);
                E.sfx(4);
            }
            public override void update()
            {
                // jumping up
                if (state == 0)
                {
                    if (y < target.Y + 16)
                    {
                        state = 1;
                        delay = 3;
                    }
                }
                // falling
                else if (state == 1)
                {
                    spd.Y += 0.5f;
                    if (spd.Y > 0 && delay > 0)
                    {
                        spd.Y = 0;
                        delay --;
                    }
                    if (spd.Y > 0 && y > target.Y)
                    {
                        y = target.Y;
                        spd = new Vector2(0, 0);
                        state = 2;
                        delay = 5;
                        G.shake = 5;
                        G.init_object(new smoke(), x, y + 4);
                        E.sfx(5);
                    }
                }
                // landing
                else if (state == 2)
                {
                    delay--;
                    spr = 6;
                    if (delay < 0)
                    {
                        G.destroy_object(this);
                        var player = G.init_object(new player(), x, y);
                        player.hair = hair;
                    }
                }
            }
            public override void draw()
            {
                hair.draw_hair(this, 1, G.max_djump);
                G.draw_player(this, G.max_djump);
            }
        }
        
        public class spring : ClassicObject
        {
            public int hide_in = 0;
            private int hide_for = 0;
            private int delay = 0;

            public override void update()
            {
                if (hide_for > 0)
                {
                    hide_for--;
                    if (hide_for <= 0)
                    {
                        spr = 18;
                        delay = 0;
                    }
                }
                else if (spr == 18)
                {
                    var hit = collide<player>(0, 0);
                    if (hit != null && hit.spd.Y >= 0)
                    {
                        spr = 19;
                        hit.y = y - 4;
                        hit.spd.X *= 0.2f;
                        hit.spd.Y = -3;
                        hit.djump = G.max_djump;
                        delay = 10;
                        G.init_object(new smoke(), x, y);

                        // breakable below us
                        var below = collide<fall_floor>(0, 1);
                        if (below != null)
                            G.break_fall_floor(below);

                        G.psfx(8);
                    }
                }
                else if (delay > 0)
                {
                    delay--;
                    if (delay <= 0)
                        spr = 18;
                }

                // begin hiding
                if (hide_in > 0)
                {
                    hide_in--;
                    if (hide_in <= 0)
                    {
                        hide_for = 60;
                        spr = 0;
                    }
                }
            }
        }

        private void break_spring(spring obj)
        {
            obj.hide_in = 15;
        }
        
        public class balloon : ClassicObject
        {
            float offset;
            float start;
            float timer;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                offset = E.rnd(1f);
                start = y;
                hitbox = new Rectangle(-1, -1, 10, 10);
            }
            public override void update()
            {
                if (spr == 22)
                {
                    offset += 0.01f;
                    y = start + E.sin(offset) * 2;
                    var hit = collide<player>(0, 0);
                    if (hit != null && hit.djump < G.max_djump)
                    {
                        G.psfx(6);
                        G.init_object(new smoke(), x, y);
                        hit.djump = G.max_djump;
                        spr = 0;
                        timer = 60;
                    }
                }
                else if (timer > 0)
                    timer--;
                else
                {
                    G.psfx(7);
                    G.init_object(new smoke(), x, y);
                    spr = 22;
                }
            }
            public override void draw()
            {
                if (spr == 22)
                {
                    E.spr(13 + (offset * 8) % 3, x, y + 6);
                    E.spr(spr, x, y);
                }
            }
        }

        public class fall_floor : ClassicObject
        {
            public int state = 0;
            public bool solid = true;
            public int delay = 0;

            public override void update()
            {
                if (state == 0)
                {
                    if (check<player>(0, -1) || check<player>(-1, 0) || check<player>(1, 0))
                        G.break_fall_floor(this);
                }
                else if (state == 1)
                {
                    delay--;
                    if (delay <= 0)
                    {
                        state = 2;
                        delay = 60; //how long it hides for
                        collideable = false;
                    }
                }
                else if (state == 2)
                {
                    delay--;
                    if (delay <= 0 && !check<player>(0, 0))
                    {
                        G.psfx(7);
                        state = 0;
                        collideable = true;
                        G.init_object(new smoke(), x, y);
                    }
                }
            }
            public override void draw()
            {
                if (state != 2)
                {
                    if (state != 1)
                        E.spr(23, x, y);
                    else
                        E.spr(23 + (15 - delay) / 5, x, y);
                }
            }
        }

        private void break_fall_floor(fall_floor obj)
        {
            if (obj.state == 0)
            {
                psfx(15);
                obj.state = 1;
                obj.delay = 15; //how long until it falls
                init_object(new smoke(), obj.x, obj.y);
                var hit = obj.collide<spring>(0, -1);
                if (hit != null)
                    break_spring(hit);
            }
        }

        public class smoke : ClassicObject
        {
            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                spr = 29;
                spd.Y = -0.1f;
                spd.X = 0.3f + E.rnd(0.2f);
                x += -1 + E.rnd(2);
                y += -1 + E.rnd(2);
                flipX = G.maybe();
                flipY = G.maybe();
                solids = false;
            }
            public override void update()
            {
                spr += 0.2f;
                if (spr >= 32)
                    G.destroy_object(this);
            }
        }
        
        public class fruit : ClassicObject
        {
            float start;
            float off;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                spr = 26;
                start = y;
                off = 0;
            }

            public override void update()
            {
                var hit = collide<player>(0, 0);
                if (hit != null)
                {
                    hit.djump = G.max_djump;
                    G.sfx_timer = 20;
                    E.sfx(13);
                    G.got_fruit.Add(1 + G.level_index());
                    G.init_object(new lifeup(), x, y);
                    G.destroy_object(this);
                    Stats.Increment(Stat.PICO_BERRIES);
                }
                off++;
                y = start + E.sin(off / 40f) * 2.5f;
            }
        }

        public class fly_fruit : ClassicObject
        {
            float start;
            bool fly = false;
            float step = 0.5f;
            float sfx_delay = 8;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                start = y;
                solids = false;
            }

            public override void update()
            {
                // fly away 
                if (fly)
                {
                    if (sfx_delay > 0)
                    {
                        sfx_delay--;
                        if (sfx_delay <= 0)
                        {
                            G.sfx_timer = 20;
                            E.sfx(14);
                        }
                    }
                    spd.Y = G.appr(spd.Y, -3.5f, 0.25f);
                    if (y < -16)
                        G.destroy_object(this);
                }
                // wait
                else
                {
                    if (G.has_dashed)
                        fly = true;
                    step += 0.05f;
                    spd.Y = E.sin(step) * 0.5f;
                }
                // collect
                var hit = collide<player>(0, 0);
                if (hit != null)
                {
                    hit.djump = G.max_djump;
                    G.sfx_timer = 20;
                    E.sfx(13);
                    G.got_fruit.Add(1 + G.level_index());
                    G.init_object(new lifeup(), x, y);
                    G.destroy_object(this);
                    Stats.Increment(Stat.PICO_BERRIES);
                }
            }
            public override void draw()
            {
                var off = 0f;
                if (!fly)
                {
                    var dir = E.sin(step);
                    if (dir < 0)
                        off = 1 + E.max(0, G.sign(y - start));
                }
                else
                    off = (off + 0.25f) % 3;
                E.spr(45 + off, x - 6, y - 2, 1, 1, true, false);
                E.spr(spr, x, y);
                E.spr(45 + off, x + 6, y - 2);
            }
        }

        public class lifeup : ClassicObject
        {
            int duration;
            float flash;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);

                spd.Y = -0.25f;
                duration = 30;
                x -= 2;
                y -= 4;
                flash = 0;
                solids = false;
            }

            public override void update()
            {
                duration--;
                if (duration <= 0)
                    G.destroy_object(this);
            }

            public override void draw()
            {
                flash += 0.5f;
                E.print("1000", x - 2, y, 7 + flash % 2);
            }
        }
        
        public class fake_wall : ClassicObject
        {
            public override void update()
            {
                hitbox = new Rectangle(-1, -1, 18, 18);
                var hit = collide<player>(0, 0);
                if (hit != null && hit.dash_effect_time > 0)
                {
                    hit.spd.X = -G.sign(hit.spd.X) * 1.5f;
                    hit.spd.Y = -1.5f;
                    hit.dash_time = -1;
                    G.sfx_timer = 20;
                    E.sfx(16);
                    G.destroy_object(this);
                    G.init_object(new smoke(), x, y);
                    G.init_object(new smoke(), x + 8, y);
                    G.init_object(new smoke(), x, y + 8);
                    G.init_object(new smoke(), x + 8, y + 8);
                    G.init_object(new fruit(), x + 4, y + 4);
                }
                hitbox = new Rectangle(0, 0, 16, 16);
            }
            public override void draw()
            {
                E.spr(64, x, y);
                E.spr(65, x + 8, y);
                E.spr(80, x, y + 8);
                E.spr(81, x + 8, y + 8);
            }
        }

        public class key : ClassicObject
        {
            public override void update()
            {
                var was = E.flr(spr);
                spr = 9 + (E.sin(G.frames / 30f) + 0.5f) * 1;
                var current = E.flr(spr);
                if (current == 10 && current != was)
                    flipX = !flipX;
                if (check<player>(0, 0))
                {
                    E.sfx(23);
                    G.sfx_timer = 20;
                    G.destroy_object(this);
                    G.has_key = true;
                }
            }
        }
        
        public class chest : ClassicObject
        {
            float start;
            float timer;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                x -= 4;
                start = x;
                timer = 20;
            }
            public override void update()
            {
                if (G.has_key)
                {
                    timer--;
                    x = start - 1 + E.rnd(3);
                    if (timer <= 0)
                    {
                        G.sfx_timer = 20;
                        E.sfx(16);
                        G.init_object(new fruit(), x, y - 4);
                        G.destroy_object(this);
                    }
                }
            }
        }

        public class platform : ClassicObject
        {
            public float dir;
            float last;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                x -= 4;
                solids = false;
                hitbox.Width = 16;
                last = x;
            }
            public override void update()
            {
                spd.X = dir * 0.65f;
                if (x < -16) x = 128;
                if (x > 128) x = -16;
                if (!check<player>(0, 0))
                {
                    var hit = collide<player>(0, -1);
                    if (hit != null)
                        hit.move_x((int)(x - last), 1);
                }
                last = x;
            }
            public override void draw()
            {
                E.spr(11, x, y - 1);
                E.spr(12, x + 8, y - 1);
            }
        }
        
        public class message : ClassicObject
        {
            float last = 0;
            float index = 0;
            public override void draw()
            {
                var text = "-- celeste mountain --#this memorial to those# perished on the climb";
                if (check<player>(4, 0))
                {
                    if (index < text.Length)
                    {
                        index += 0.5f;
                        if (index >= last + 1)
                        {
                            last += 1;
                            E.sfx(35);
                        }
                    }

                    var off = new Vector2(8, 96);
                    for (var i = 0; i < index; i ++)
                    {
                        if (text[i] != '#')
                        {
                            E.rectfill(off.X - 2, off.Y - 2, off.X + 7, off.Y + 6, 7);
                            E.print("" + text[i], off.X, off.Y, 0);
                            off.X += 5;
                        }
                        else
                        {
                            off.X = 8;
                            off.Y += 7;
                        }
                    }
                }
                else
                {
                    index = 0;
                    last = 0;
                }
            }
        }
        
        public class big_chest : ClassicObject
        {
            int state = 0;
            float timer;

            private class particle
            {
                public float x;
                public float y;
                public float h;
                public float spd;
            }
            private List<particle> particles;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                hitbox.Width = 16;
            }
            public override void draw()
            {
                if (state == 0)
                {
                    var hit = collide<player>(0, 8);
                    if (hit !=null && hit.is_solid(0, 1))
                    {
                        E.music(-1, 500, 7);
                        E.sfx(37);
                        G.pause_player = true;
                        hit.spd.X = 0;
                        hit.spd.Y = 0;
                        state = 1;
                        G.init_object(new smoke(), x, y);
                        G.init_object(new smoke(), x + 8, y);
                        timer = 60;
                        particles = new List<particle>();
                    }
                    E.spr(96, x, y);
                    E.spr(97, x + 8, y);
                }
                else if (state == 1)
                {
                    timer--;
                    G.shake = 5;
                    G.flash_bg = true;
                    if (timer <= 45 && particles.Count < 50)
                    {
                        particles.Add(new particle()
                        {
                            x = 1 + E.rnd(14),
                            y = 0,
                            h = 32+E.rnd(32),
                            spd = 8+E.rnd(8)
                        });
                    }
                    if (timer < 0)
                    {
                        state = 2;
                        particles.Clear();
                        G.flash_bg = false;
                        G.new_bg = true;
                        G.init_object(new orb(), x + 4, y + 4);
                        G.pause_player = false;
                    }
                    foreach (var p in particles)
                    {
                        p.y += p.spd;
                        E.rectfill(x + p.x, y + 8 - p.y, x + p.x + 1, E.min(y + 8 - p.y + p.h, y + 8), 7);
                    }
                }

                E.spr(112, x, y + 8);
                E.spr(113, x + 8, y + 8);
            }
        }

        public class orb : ClassicObject
        {
            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spd.Y = -4;
                solids = false;
            }
            public override void draw()
            {
                spd.Y = G.appr(spd.Y, 0, 0.5f);
                var hit = collide<player>(0, 0);
                if (spd.Y == 0 && hit != null)
                {
                    G.music_timer = 45;
                    E.sfx(51);
                    G.freeze = 10;
                    G.shake = 10;
                    G.destroy_object(this);
                    G.max_djump = 2;
                    hit.djump = 2;
                }

                E.spr(102, x, y);
                var off = G.frames / 30f;
                for (var i = 0; i <= 7; i++)
                    E.circfill(x + 4 + E.cos(off + i / 8f) * 8, y + 4 + E.sin(off + i / 8f) * 8, 1, 7);
            }
        }
        
        public class flag : ClassicObject
        {
            float score = 0;
            bool show = false;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                x += 5;
                score = G.got_fruit.Count;

                Stats.Increment(Stat.PICO_COMPLETES);
                Achievements.Register(Achievement.PICO8);
            }
            public override void draw()
            {
                spr = 118 + (G.frames / 5f) % 3;
                E.spr(spr, x, y);
                if (show)
                {
                    E.rectfill(32, 2, 96, 31, 0);
                    E.spr(26, 55, 6);
                    E.print("x" + score, 64, 9, 7);
                    G.draw_time(49, 16);
                    E.print("deaths:" + G.deaths, 48, 24, 7);
                }
                else if (check<player>(0, 0))
                {
                    E.sfx(55);
                    G.sfx_timer = 30;
                    show = true;
                }
            }
        }

        public class room_title : ClassicObject
        {
            float delay = 5;
            public override void draw()
            {
                delay--;
                if (delay < -30)
                    G.destroy_object(this);
                else if (delay < 0)
                {
                    E.rectfill(24, 58, 104, 70, 0);
                    if (G.room.X == 3 && G.room.Y == 1)
                        E.print("old site", 48, 62, 7);
                    else if (G.level_index() == 30)
                        E.print("summit", 52, 62, 7);
                    else
                    {
                        var level = (1 + G.level_index()) * 100;
                        E.print(level + "m", 52 + (level < 1000 ? 2 : 0), 62, 7);
                    }

                    G.draw_time(4, 4);
                }
            }
        }

        #endregion

        #region object functions

        public class ClassicObject
        {
            public Classic G;
            public Emulator E;

            public int type;
            public bool collideable = true;
            public bool solids = true;
            public float spr;
            public bool flipX;
            public bool flipY;
            public float x;
            public float y;
            public Rectangle hitbox = new Rectangle(0, 0, 8, 8);
            public Vector2 spd = new Vector2(0, 0);
            public Vector2 rem = new Vector2(0, 0);

            public virtual void init(Classic g, Emulator e)
            {
                G = g;
                E = e;
            }

            public virtual void update()
            {

            }

            public virtual void draw()
            {
                if (spr > 0)
                    E.spr(spr, x, y, 1, 1, flipX, flipY);
            }

            public bool is_solid(int ox, int oy)
            {
                if (oy > 0 && !check<platform>(ox, 0) && check<platform>(ox, oy))
                    return true;
                return G.solid_at(x + hitbox.X + ox, y + hitbox.Y + oy, hitbox.Width, hitbox.Height) ||
                    check<fall_floor>(ox, oy) ||
                    check<fake_wall>(ox, oy);
            }

            public bool is_ice(int ox, int oy)
            {
                return G.ice_at(x + hitbox.X + ox, y + hitbox.Y + oy, hitbox.Width, hitbox.Height);
            }

            public T collide<T>(int ox, int oy) where T : ClassicObject
            {
                var type = typeof(T);
                foreach (var other in G.objects)
                {
                    if (other != null && other.GetType() == type && other != this && other.collideable &&
                        other.x + other.hitbox.X + other.hitbox.Width > x + hitbox.X + ox &&
                        other.y + other.hitbox.Y + other.hitbox.Height > y + hitbox.Y + oy &&
                        other.x + other.hitbox.X < x + hitbox.X + hitbox.Width + ox &&
                        other.y + other.hitbox.Y < y + hitbox.Y + hitbox.Height + oy)
                        return other as T;

                }
                return null;
            }

            public bool check<T>(int ox, int oy) where T : ClassicObject
            {
                return collide<T>(ox, oy) != null;
            }

            public void move(float ox, float oy)
            {
                int amount = 0;
                // [x] get move amount
                rem.X += ox;
                amount = E.flr(rem.X + 0.5f);
                rem.X -= amount;
                move_x(amount, 0);

                // [y] get move amount
                rem.Y += oy;
                amount = E.flr(rem.Y + 0.5f);
                rem.Y -= amount;
                move_y(amount);
            }

            public void move_x(int amount, int start)
            {
                if (solids)
                {
                    var step = G.sign(amount);
                    for (int i = start; i <= E.abs(amount); i++)
                    {
                        if (!is_solid(step, 0))
                            x += step;
                        else
                        {
                            spd.X = 0;
                            rem.X = 0;
                            break;
                        }
                    }
                }
                else
                    x += amount;
            }

            public void move_y(int amount)
            {
                if (solids)
                {
                    var step = G.sign(amount);
                    for (var i = 0; i <= E.abs(amount); i++)
                        if (!is_solid(0, step))
                            y += step;
                        else
                        {
                            spd.Y = 0;
                            rem.Y = 0;
                            break;
                        }
                }
                else
                    y += amount;
            }

        }

        private T init_object<T>(T obj, float x, float y, int? tile = null) where T : ClassicObject
        {
            objects.Add(obj);
            if (tile.HasValue)
                obj.spr = tile.Value;
            obj.x = (int)x;
            obj.y = (int)y;
            obj.init(this, E);

            return obj;
        }

        private void destroy_object(ClassicObject obj)
        {
            var index = objects.IndexOf(obj);
            if (index >= 0)
                objects[index] = null;
        }

        private void kill_player(player obj)
        {
            sfx_timer = 12;
            E.sfx(0);
            deaths++;
            shake = 10;
            destroy_object(obj);
            Stats.Increment(Stat.PICO_DEATHS);

            dead_particles.Clear();
            for (var dir = 0; dir <= 7; dir ++)
            {
                var angle = (dir / 8f);
                dead_particles.Add(new DeadParticle()
                {
                    x = obj.x + 4,
                    y = obj.y + 4,
                    t = 10,
                    spd = new Vector2(E.cos(angle) * 3, E.sin(angle + 0.5f) * 3)
                });
            }

            restart_room();
        }

        #endregion

        #region room functions

        private void restart_room()
        {
            will_restart = true;
            delay_restart = 15;
        }

        private void next_room()
        {
            if (room.X == 2 && room.Y == 1)
                E.music(30, 500, 7);
            else if (room.X == 3 && room.Y == 1)
                E.music(20, 500, 7);
            else if (room.X == 4 && room.Y == 2)
                E.music(30, 500, 7);
            else if (room.X == 5 && room.Y == 3)
                E.music(30, 500, 7);

            if (room.X == 7)
                load_room(0, room.Y + 1);
            else
                load_room(room.X + 1, room.Y);
        }

        public void load_room(int x, int y)
        {
            has_dashed = false;
            has_key = false;

            // remove existing objects
            for (int i = 0; i < objects.Count; i++)
                objects[i] = null;

            // current room
            room.X = x;
            room.Y = y;

            // entities
            for (int tx = 0; tx <= 15; tx ++)
            {
                for (int ty = 0; ty <= 15; ty ++)
                {
                    var tile = E.mget(room.X * 16 + tx, room.Y * 16 + ty);
                    if (tile == 11)
                        init_object(new platform(), tx * 8, ty * 8).dir = -1;
                    else if (tile == 12)
                        init_object(new platform(), tx * 8, ty * 8).dir = 1;
                    else
                    {
                        ClassicObject obj = null;

                        if (tile == 1)
                            obj = new player_spawn();
                        else if (tile == 18)
                            obj = new spring();
                        else if (tile == 22)
                            obj = new balloon();
                        else if (tile == 23)
                            obj = new fall_floor();
                        else if (tile == 86)
                            obj = new message();
                        else if (tile == 96)
                            obj = new big_chest();
                        else if (tile == 118)
                            obj = new flag();
                        else if (!got_fruit.Contains(1 + level_index()))
                        {
                            if (tile == 26)
                                obj = new fruit();
                            else if (tile == 28)
                                obj = new fly_fruit();
                            else if (tile == 64)
                                obj = new fake_wall();
                            else if (tile == 8)
                                obj = new key();
                            else if (tile == 20)
                                obj = new chest();
                        }

                        if (obj != null)
                            init_object(obj, tx * 8, ty * 8, tile);
                    }
                }
            }

            if (!is_title())
                init_object(new room_title(), 0, 0);
        }

        #endregion

        #region update

        public void Update()
        {
            frames = ((frames + 1) % 30);
            if (frames == 0 && level_index() < 30)
            {
                seconds = ((seconds + 1) % 60);
                if (seconds == 0)
                    minutes++;
            }

            if (music_timer > 0)
            {
                music_timer--;
                if (music_timer <= 0)
                    E.music(10, 0, 7);
            }

            if (sfx_timer > 0)
                sfx_timer--;

            // cancel if freeze
            if (freeze > 0)
            {
                freeze--;
                return;
            }

            // screenshake
            if (shake > 0 && !Settings.Instance.DisableScreenShake)
            {
                shake--;
                E.camera();
                if (shake > 0)
                    E.camera(-2 + E.rnd(5), -2 + E.rnd(5));
            }

            // restart(soon)
            if (will_restart && delay_restart > 0)
            {
                delay_restart--;
                if (delay_restart <= 0)
                {
                    will_restart = true;
                    load_room(room.X, room.Y);
                }
            }

            // update each object
            int length = objects.Count;
            for (var i = 0; i < length; i ++)
            {
                var obj = objects[i];
                if (obj != null)
                {
                    obj.move(obj.spd.X, obj.spd.Y);
                    obj.update();
                }
            }

            // C# NEW CODE:
            // clear deleted objects
            while (objects.IndexOf(null) >= 0)
                objects.Remove(null);

            // start game
            if (is_title())
            {
                if (!start_game && (E.btn(k_jump) || E.btn(k_dash)))
                {
                    E.music(-1, 0, 0);
                    start_game_flash = 50;
                    start_game = true;
                    E.sfx(38);
                }
                if (start_game)
                {
                    start_game_flash--;
                    if (start_game_flash <= -30)
                        begin_game();
                }
            }
        }

        #endregion

        #region drawing
        
        public void Draw()
        {
            // reset all palette values
            E.pal();

            // start game flash
            if (start_game)
            {
                var c = 10;
                if (start_game_flash > 10)
                {
                    if (frames % 10 < 5)
                        c = 7;
                }
                else if (start_game_flash > 5)
                    c = 2;
                else if (start_game_flash > 0)
                    c = 1;
                else
                    c = 0;

                if (c < 10)
                {
                    E.pal(6, c);
                    E.pal(12, c);
                    E.pal(13, c);
                    E.pal(5, c);
                    E.pal(1, c);
                    E.pal(7, c);
                }
            }

            // clear screen
            var bg_col = 0;
            if (flash_bg)
                bg_col = frames / 5;
            else if (new_bg)
                bg_col = 2;
            E.rectfill(0, 0, 128, 128, bg_col);

            // clouds
            if (!is_title())
            {
                foreach (var c in clouds)
                {
                    c.x += c.spd;
                    E.rectfill(c.x, c.y, c.x + c.w, c.y + 4 + (1 - c.w / 64) * 12, new_bg ? 14 : 1);
                    if (c.x > 128)
                    {
                        c.x = -c.w;
                        c.y = E.rnd(128 - 8);
                    }
                }
            }

            // draw bg terrain
            E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 2);

            // platforms / big chest
            for (var i = 0; i < objects.Count; i++)
            {
                var o = objects[i];
                if (o != null && (o is platform || o is big_chest))
                    draw_object(o);
            }
            
            // draw terrain
            var off = is_title() ? -4 : 0;
            E.map(room.X * 16, room.Y * 16, off, 0, 16, 16, 1);

            // draw objects
            for (var i = 0; i < objects.Count; i++)
            {
                var o = objects[i];
                if (o != null && !(o is platform) && !(o is big_chest))
                    draw_object(o);
            }

            // draw fg terrain
            E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 3);

            // particles
            foreach (var p in particles)
            {
                p.x += p.spd;
                p.y += E.sin(p.off);
                p.off += E.min(0.05f, p.spd / 32);
                E.rectfill(p.x, p.y, p.x + p.s, p.y + p.s, p.c);
                if (p.x > 128 + 4)
                {
                    p.x = -4;
                    p.y = E.rnd(128);
                }
            }

            // dead particles
            for (int i = dead_particles.Count - 1; i >= 0; i--)
            {
                var p = dead_particles[i];
                p.x += p.spd.X;
                p.y += p.spd.Y;
                p.t--;
                if (p.t <= 0)
                    dead_particles.RemoveAt(i);
                E.rectfill(p.x - p.t / 5, p.y - p.t / 5, p.x + p.t / 5, p.y + p.t / 5, 14 + p.t % 2);
            }

            // draw outside of the screen for screenshake
            E.rectfill(-5, -5, -1, 133, 0);
            E.rectfill(-5, -5, 133, -1, 0);
            E.rectfill(-5, 128, 133, 133, 0);
            E.rectfill(128, -5, 133, 133, 0);
            
            // C# Change: "press button" instead to fit consoles
            // no need for credits here
            if (is_title())
            {
                E.print("press button", 42, 96, 5);
                //E.print("matt thorson", 42, 96, 5);
                //E.print("noel berry", 46, 102, 5);
            }

            if (level_index() == 30)
            {
                ClassicObject p = null;

                foreach (var o in objects)
                    if (o is player)
                    {
                        p = o;
                        break;
                    }

                if (p != null)
                {
                    var diff = E.min(24, 40 - E.abs(p.x + 4 - 64));
                    E.rectfill(0, 0, diff, 128, 0);
                    E.rectfill(128 - diff, 0, 128, 128, 0);
                }
            }
        }

        private void draw_object(ClassicObject obj)
        {
            obj.draw();
        }

        private void draw_time(int x, int y)
        {
            var s = seconds;
            var m = minutes % 60;
            var h = E.flr(minutes / 60);

            E.rectfill(x, y, x + 32, y + 6, 0);
            E.print((h < 10 ? "0" : "") + h + ":" + (m < 10 ? "0" : "") + m + ":" + (s < 10 ? "0" : "") + s, x + 1, y + 1,7);
        }

        #endregion

        #region util

        private float clamp(float val, float a, float b)
        {
            return E.max(a, E.min(b, val));
        }

        private float appr(float val, float target, float amount)
        {
            return (val > target ? E.max(val - amount, target) : E.min(val + amount, target));
        }

        private int sign(float v)
        {
            return (v > 0 ? 1 : (v < 0 ? -1 : 0));
        }

        private bool maybe()
        {
            return E.rnd(1) < 0.5f;
        }

        private bool solid_at(float x, float y, float w, float h)
        {
            return tile_flag_at(x, y, w, h, 0);
        }

        private bool ice_at(float x, float y, float w, float h)
        {
            return tile_flag_at(x, y, w, h, 4);
        }

        private bool tile_flag_at(float x, float y, float w, float h, int flag)
        {
            for (var i = (int)E.max(0, E.flr(x / 8f)); i <= E.min(15, (x + w - 1) / 8); i++)
                for (var j = (int)E.max(0, E.flr(y / 8f)); j <= E.min(15, (y + h - 1) / 8); j ++)
                    if (E.fget(tile_at(i, j), flag))
                        return true;
            return false;
        }

        private int tile_at(int x, int y)
        {
            return E.mget(room.X * 16 + x, room.Y * 16 + y);
        }

        private bool spikes_at(float x, float y, int w, int h, float xspd, float yspd)
        {
            for (var i = (int)E.max(0, E.flr(x / 8f)); i <= E.min(15, (x + w - 1) / 8); i++)
                for (var j = (int)E.max(0, E.flr(y / 8f)); j <= E.min(15, (y + h - 1) / 8); j++)
                {
                    var tile = tile_at(i, j);
                    if (tile == 17 && ((y + h - 1) % 8 >= 6 || y + h == j * 8 + 8) && yspd >= 0)
                        return true;
                    else if (tile == 27 && y % 8 <= 2 && yspd <= 0)
                        return true;
                    else if (tile == 43 && x % 8 <= 2 && xspd <= 0)
                        return true;
                    else if (tile == 59 && ((x + w - 1) % 8 >= 6 || x + w == i * 8 + 8) && xspd >= 0)
                        return true;
                }
            return false;
        }

        #endregion
    }
}
