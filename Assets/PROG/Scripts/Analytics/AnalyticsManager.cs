using Unity.Services.Analytics;
using UnityEngine;
using Event = Unity.Services.Analytics.Event;

namespace Wendogo
{
    public class AnalyticsManager : MonoBehaviour
    {
        #region Analytics Manager Instance

        private static AnalyticsManager _instance;

        public static AnalyticsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }

        private static void CreateInstance()
        {
            var gameObject = new GameObject($"{nameof(AnalyticsManager)}");
            _instance = gameObject.AddComponent<AnalyticsManager>();
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Variables
        
        private IAnalyticsService _analyticsService;
        public IAnalyticsService AnalyticsService
        {
            get { return _analyticsService ??= Unity.Services.Analytics.AnalyticsService.Instance; }
            private set => _analyticsService = value;
        }

        #endregion

        public void StartDataCollection()
        {
            if (AnalyticsService != null)
                AnalyticsService.StartDataCollection();
        }

        public void RecordEvent(Event e)
        {
            AnalyticsService.RecordEvent(e);
        }
    }
}