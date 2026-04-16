using System;
using System.Collections.Generic;
using UnityEngine;
using CriminalCase2.Utils;

namespace CriminalCase2.Utils
{
    /// <summary>
    /// Service Locator pattern implementation for dependency injection.
    /// Replaces singleton pattern with a centralized service registry.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly Dictionary<Type, List<Action<object>>> _pendingRegistrations = new();

        /// <summary>
        /// Register a service instance.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                LoggingUtility.Warning("ServiceLocator", $"Service of type {type.Name} is already registered. Overwriting.");
            }
            
            _services[type] = service;
            LoggingUtility.LogDebug("ServiceLocator", $"Registered service: {type.Name}");
            
            // Fulfill any pending registration callbacks
            if (_pendingRegistrations.TryGetValue(type, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(service);
                }
                _pendingRegistrations.Remove(type);
            }
        }

        /// <summary>
        /// Unregister a service.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.Remove(type))
            {
                LoggingUtility.LogDebug("ServiceLocator", $"Unregistered service: {type.Name}");
            }
        }

        /// <summary>
        /// Get a registered service. Returns null if not found.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            
            LoggingUtility.Warning("ServiceLocator", $"Service of type {type.Name} not found!");
            return null;
        }

        /// <summary>
        /// Try to get a registered service.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            service = Get<T>();
            return service != null;
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Register a callback to be invoked when a service becomes available.
        /// If service is already registered, callback is invoked immediately.
        /// </summary>
        public static void WhenAvailable<T>(Action<T> callback) where T : class
        {
            var type = typeof(T);
            
            // If already registered, invoke immediately
            if (_services.TryGetValue(type, out var service))
            {
                callback?.Invoke(service as T);
                return;
            }
            
            // Otherwise, queue for later
            if (!_pendingRegistrations.TryGetValue(type, out var callbacks))
            {
                callbacks = new List<Action<object>>();
                _pendingRegistrations[type] = callbacks;
            }
            
            callbacks.Add(obj => callback?.Invoke(obj as T));
        }

        /// <summary>
        /// Clear all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _pendingRegistrations.Clear();
            LoggingUtility.Info("ServiceLocator", "All services cleared");
        }

        /// <summary>
        /// Get count of registered services.
        /// </summary>
        public static int ServiceCount => _services.Count;
    }

    /// <summary>
    /// MonoBehaviour extension for easy service registration.
    /// </summary>
    public abstract class ServiceBase<T> : MonoBehaviour where T : class
    {
        protected virtual void Awake()
        {
            RegisterService();
        }

        protected virtual void OnDestroy()
        {
            UnregisterService();
        }

        protected virtual void RegisterService()
        {
            ServiceLocator.Register<T>(this as T);
        }

        protected virtual void UnregisterService()
        {
            ServiceLocator.Unregister<T>();
        }
    }
}
