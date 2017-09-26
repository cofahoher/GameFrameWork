using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class RenderMessageType
    {
        //命名是：做（什么）
        public const int StartMoving = 101;               //LocomoteRenderMessage
        public const int StopMoving = 102;                //LocomoteRenderMessage
        public const int ChangeLocomotorSpeed = 103;      //ChangeLocomotorSpeedRenderMessage
        public const int ChangeDirection = 104;           //ChangeDirectionRenderMessage
        public const int ChangePosition = 105;            //ChangePositionRenderMessage
        public const int ChangeMana = 109;                //ChangeManaRenderMessage
        public const int ChangeHealth = 110;              //ChangeHealthRenderMessage
        public const int Die = 111;                       //SimpleRenderMessage
        public const int Hide = 112;                      //SimpleRenderMessage
        public const int Show = 113;                      //SimpleRenderMessage
        public const int PlayAnimation = 120;             //PlayAnimationRenderMessage
        public const int InterruptAnimation = 121;        //InterruptAnimationRenderMessage
        public const int PlayRenderEffect = 130;          //PlayRenderEffectMessage
        public const int PlaySound = 131;                 //PlaySoundMessage
        public const int TakeDamage = 140;                //TakeDamageRenderMessage
        public const int ChangeLevel = 150;               //SimpleRenderMessage
        public const int AddExperience = 151;             //SimpleRenderMessage
        public const int ChangeAttackSpeed = 160;         //SimpleRenderMessage
        public const int ChangeCooldDownReduce = 161;     //SimpleRenderMessage

        public const int PlayerChangeFaction = 1001;      //SimpleRenderMessage
        public const int ChangeCameraPosition = 1002;     //ChangeCameraPositionRenderMessage

        public const int GameOver = 2000;                 //GameOverRenderMessage
    }

    public class LocomoteRenderMessage : RenderMessage
    {
        public const int NotFromCommand = 1;
        public const int NotLocomotion = 2;

        public bool m_block_animation = false;
        public int m_reason = 0;

        public void ConstructAsStartMoving(int entity_id, bool block_animation = false, int reason = 0)
        {
            m_type = RenderMessageType.StartMoving;
            m_entity_id = entity_id;
            m_block_animation = block_animation;
            m_reason = reason;
        }

        public void ConstructAsStopMoving(int entity_id, bool block_animation = false, int reason = 0)
        {
            m_type = RenderMessageType.StopMoving;
            m_entity_id = entity_id;
            m_block_animation = block_animation;
            m_reason = reason;
        }

        public override void Reset()
        {
            m_type = RenderMessageType.Invalid;
            m_entity_id = -1;
            m_block_animation = false;
            m_reason = 0;
        }
    }

    public class ChangeLocomotorSpeedRenderMessage : RenderMessage
    {
        public FixPoint m_animation_rate = FixPoint.Zero;

        public ChangeLocomotorSpeedRenderMessage()
        {
            m_type = RenderMessageType.ChangeLocomotorSpeed;
        }

        public void Construct(int entity_id, FixPoint animation_rate)
        {
            m_entity_id = entity_id;
            m_animation_rate = animation_rate;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_animation_rate = FixPoint.Zero;
        }
    }

    public class ChangeDirectionRenderMessage : RenderMessage
    {
        public FixPoint m_base_angle = FixPoint.Zero;
        public FixPoint m_head_angle = FixPoint.Zero;

        public ChangeDirectionRenderMessage()
        {
            m_type = RenderMessageType.ChangeDirection;
        }

        public void Construct(int entity_id, FixPoint base_angle, FixPoint head_angle)
        {
            m_entity_id = entity_id;
            m_base_angle = base_angle;
            m_head_angle = head_angle;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_base_angle = FixPoint.Zero;
            m_head_angle = FixPoint.Zero;
        }
    }
    
    public class ChangePositionRenderMessage : RenderMessage
    {
        public Vector3FP m_new_position;

        public ChangePositionRenderMessage()
        {
            m_type = RenderMessageType.ChangePosition;
        }

        public void Construct(int entity_id, Vector3FP new_position)
        {
            m_entity_id = entity_id;
            m_new_position = new_position;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_new_position.MakeZero();
        }
    }

    public class ChangeManaRenderMessage : RenderMessage
    {
        public int m_mana_type = 0;
        public FixPoint m_delta_mana = FixPoint.Zero;
        public FixPoint m_current_mana = FixPoint.Zero;

        public ChangeManaRenderMessage()
        {
            m_type = RenderMessageType.ChangeMana;
        }

        public void Construct(int entity_id, int mana_type, FixPoint delta_mana, FixPoint current_mana)
        {
            m_entity_id = entity_id;
            m_mana_type = mana_type;
            m_delta_mana = delta_mana;
            m_current_mana = current_mana;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_mana_type = 0;
            m_delta_mana = FixPoint.Zero;
            m_current_mana = FixPoint.Zero;
        }
    }

    public class ChangeHealthRenderMessage : RenderMessage
    {
        public FixPoint m_delta_health = FixPoint.Zero;
        public FixPoint m_current_health = FixPoint.Zero;

        public ChangeHealthRenderMessage()
        {
            m_type = RenderMessageType.ChangeHealth;
        }

        public void Construct(int entity_id, FixPoint delta_health, FixPoint current_health)
        {
            m_entity_id = entity_id;
            m_delta_health = delta_health;
            m_current_health = current_health;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_delta_health = FixPoint.Zero;
            m_current_health = FixPoint.Zero;
        }
    }

    public class PlayAnimationRenderMessage : RenderMessage
    {
        public string m_animation_name;
        public string m_animation_name_2;
        public bool m_loop;
        public FixPoint m_speed;

        public PlayAnimationRenderMessage()
        {
            m_type = RenderMessageType.PlayAnimation;
        }

        public void Construct(int entity_id, string animation_name, string animation_name_2, bool loop, FixPoint speed)
        {
            m_entity_id = entity_id;
            m_animation_name = animation_name;
            m_animation_name_2 = animation_name_2;
            m_loop = loop;
            m_speed = speed;
        }

        public override void Reset()
        {
            m_entity_id = -1;
        }
    }

    public class InterruptAnimationRenderMessage : RenderMessage
    {
        public string m_if_this_animation;
        public string m_play_this_animation;
        public bool m_loop;

        public InterruptAnimationRenderMessage()
        {
            m_type = RenderMessageType.InterruptAnimation;
        }

        public void Construct(int entity_id, string if_this_animation, string play_this_animation = null, bool loop = false)
        {
            m_entity_id = entity_id;
            m_if_this_animation = if_this_animation;
            m_play_this_animation = play_this_animation;
            m_loop = loop;
        }

        public override void Reset()
        {
            m_entity_id = -1;
        }
    }

    public class PlayRenderEffectMessage : RenderMessage
    {
        public int m_effect_cfgid = 0;
        public FixPoint m_play_time = FixPoint.Zero;
        public bool m_play = true;

        public PlayRenderEffectMessage()
        {
            m_type = RenderMessageType.PlayRenderEffect;
        }

        public void ConstructAsPlay(int entity_id, int effect_cfgid, FixPoint play_time)
        {
            m_entity_id = entity_id;
            m_effect_cfgid = effect_cfgid;
            m_play_time = play_time;
            m_play = true;
        }

        public void ConstructAsStop(int entity_id, int effect_cfgid)
        {
            m_entity_id = entity_id;
            m_effect_cfgid = effect_cfgid;
            m_play = false;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_effect_cfgid = 0;
            m_play_time = FixPoint.Zero;
            m_play = false;
        }
    }

    public class PlaySoundMessage : RenderMessage
    {
        public int m_sound_cfgid = 0;
        public FixPoint m_play_time = FixPoint.Zero;

        public PlaySoundMessage()
        {
            m_type = RenderMessageType.PlaySound;
        }

        public void Construct(int entity_id, int sound_cfgid, FixPoint play_time)
        {
            m_entity_id = entity_id;
            m_sound_cfgid = sound_cfgid;
            m_play_time = play_time;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_sound_cfgid = 0;
            m_play_time = FixPoint.Zero;
        }
    }

    public class TakeDamageRenderMessage : RenderMessage
    {
        public FixPoint m_origina_damage_amount;
        public FixPoint m_final_damage_amount;
        public int m_damage_render_effect_cfgid;
        public int m_damage_sound_cfgid;

        public TakeDamageRenderMessage()
        {
            m_type = RenderMessageType.TakeDamage;
        }

        public void Construct(int entity_id, FixPoint origina_damage_amount, FixPoint final_damage_amount, int damage_render_effect_cfgid, int damage_sound_cfgid)
        {
            m_entity_id = entity_id;
            m_origina_damage_amount = origina_damage_amount;
            m_final_damage_amount = final_damage_amount;
            m_damage_render_effect_cfgid = damage_render_effect_cfgid;
            m_damage_sound_cfgid = damage_sound_cfgid;
        }

        public override void Reset()
        {
            m_entity_id = -1;
        }
    }

    public class ChangeCameraPositionRenderMessage : RenderMessage
    {
        public bool m_flash_move = false;
        public int m_before_room_id;
        public int m_next_room_id;
        public FixPoint m_x_position;
        public FixPoint m_z_position;
        public ChangeCameraPositionRenderMessage()
        {
            m_type = RenderMessageType.ChangeCameraPosition;
        }

        public void Construct(int before_id,int next_id,FixPoint x,FixPoint z,bool flash_move)
        {
            m_flash_move = flash_move;
            m_before_room_id = before_id;
            m_next_room_id = next_id;
            m_x_position = x;
            m_z_position = z;
        }

        public override void Reset()
        {
            m_entity_id = -1;
        }
    }
}