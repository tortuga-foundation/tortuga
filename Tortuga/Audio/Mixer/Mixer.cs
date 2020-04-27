using Tortuga.Components;
using System.Collections.Generic;
using Tortuga.Audio.Effect;
using System;

namespace Tortuga.Audio
{
    /// <summary>
    /// Mixer group, can be used to pipe the audio from a audio source component
    /// </summary>
    public class MixerGroup
    {
        internal Action OnMixerEffectsUpdated;
        internal Action OnMixerSettingsUpdated;

        /// <summary>
        /// name of this mixer group
        /// </summary>
        public string Name;

        /// <summary>
        /// gain of this mixer group
        /// </summary>
        public float Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                OnMixerSettingsUpdated?.Invoke();
            }
        }
        private float _gain;
        /// <summary>
        /// pitch of this mixer group
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                OnMixerSettingsUpdated?.Invoke();
            }
        }
        private float _pitch;

        /// <summary>
        /// get's the gain multiplied by parent's gain
        /// </summary>
        public float FullGain
        {
            get
            {
                if (_parent == null)
                    return _gain;
                return _parent.FullGain * _gain;
            }
        }

        /// <summary>
        /// get's the pitch multiplied by parent's pitch
        /// </summary>
        public float FullPitch
        {
            get
            {
                if (_parent == null)
                    return _pitch;
                return _parent.FullPitch * _pitch;
            }
        }

        /// <summary>
        /// child mixer groups
        /// </summary>
        public MixerGroup[] Children => _children.ToArray();
        private List<MixerGroup> _children;

        /// <summary>
        /// effects to apply the audio sources
        /// </summary>
        public AudioEffect[] Effects => _effects.ToArray();
        private List<AudioEffect> _effects;

        /// <summary>
        /// get's the full effects including parent effects
        /// </summary>
        public List<AudioEffect> FullEffects
        {
            get
            {
                if (_parent == null)
                    return _effects;
                
                var fullEffects = _effects;
                foreach (var ef in _parent.Effects)
                    fullEffects.Add(ef);
                return fullEffects;
            }
        }

        /// <summary>
        /// the parent of this mixer group
        /// </summary>
        public MixerGroup Parent => _parent;
        private MixerGroup _parent;

        /// <summary>
        /// constructor for mixer group
        /// </summary>
        public MixerGroup()
        {
            _children = new List<MixerGroup>();
            _effects = new List<AudioEffect>();
            OnMixerEffectsUpdated += MixerRecursiveUpdate;
            _gain = 1.0f;
            _pitch = 1.0f;
        }
        /// <summary>
        /// deconstructor for mixer group
        /// </summary> 
        ~MixerGroup()
        {
            OnMixerEffectsUpdated -= MixerRecursiveUpdate;
        }

        private void MixerRecursiveUpdate()
        {
            foreach (var child in Children)
                child.OnMixerEffectsUpdated?.Invoke();
        }

        /// <summary>
        /// add a child mixer group
        /// </summary>
        /// <param name="group">the mixer group to add as a child</param>
        public void AddChild(MixerGroup group)
        {
            _children.Add(group);
            group._parent = this;
        }
        /// <summary>
        /// removes a child mixer group
        /// </summary>
        /// <param name="group">the mixer group to remove from children</param>
        public void RemoveChild(MixerGroup group)
        {
            _children.Remove(group);
            group._parent = null;
        }

        /// <summary>
        /// Add an effect to this mixer group
        /// </summary>
        /// <param name="effect">effect to add</param>
        public void AddEffect(AudioEffect effect)
        {
            _effects.Add(effect);
            OnMixerEffectsUpdated?.Invoke();
        }

        /// <summary>
        /// Removes an effect added to this mixer group
        /// </summary>
        /// <param name="effect">effect to remove</param>
        public void RemoveEffect(AudioEffect effect)
        {
            _effects.Remove(effect);
            OnMixerEffectsUpdated?.Invoke();
        }
    }
}