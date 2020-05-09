﻿using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System.Linq;

namespace Pixel.Automation.Core.Components
{
    public static class EntityManagerExtensions
    {

        public static IApplication GetApplicationDetails(this IEntityManager entityManager, IComponent childComponent)
        {
            //This will allow IControlLocator<T> or ICoordinate provider to get IApplication since they are immediate child of ApplicationDetailsEntity for default cases
            if (childComponent.Parent is ApplicationEntity)
            {
                return (childComponent.Parent as ApplicationEntity).GetTargetApplicationDetails();
            }

            ApplicationEntity targetApp = GetApplicationDetailsEntity(entityManager, childComponent);
            return targetApp.GetTargetApplicationDetails();

        }

        public static T GetApplicationDetails<T>(this IEntityManager entityManager, IComponent childComponent) where T : class, IApplication
        {
            //This will allow IControlLocator<T> or ICoordinate provider to get IApplication since they are immediate child of ApplicationDetailsEntity for default cases
            if (childComponent.Parent is ApplicationEntity)
            {
                return (childComponent.Parent as ApplicationEntity).GetTargetApplicationDetails<T>();
            }

            ApplicationEntity targetApp = GetApplicationDetailsEntity(entityManager, childComponent);
            return targetApp.GetTargetApplicationDetails<T>();

        }

        public static IControlLocator GetControlLocator(this IEntityManager entityManager, IControlIdentity forControl)
        {
            ApplicationEntity applicationDetails = entityManager.GetApplicationEntityByApplicationId(forControl.ApplicationId);
            var controlLocator = applicationDetails.GetComponentsOfType<IControlLocator>().
                Single(c => c.CanProcessControlOfType(forControl));         
            return controlLocator;        
        }

        public static ICoordinateProvider GetCoordinateProvider(this IEntityManager entityManager, IControlIdentity forControl)
        {
            ApplicationEntity applicationDetails = entityManager.GetApplicationEntityByApplicationId(forControl.ApplicationId);
            var coordinateProvider = applicationDetails.GetComponentsOfType<ICoordinateProvider>().
                Single(c => c.CanProcessControlOfType(forControl));
            return coordinateProvider;
        }

        public static ApplicationEntity GetApplicationEntityByApplicationId(this IEntityManager entityManager, string applicationId)
        {
            var applicationsInPool = (entityManager.RootEntity.GetComponentsByTag("ApplicationPoolEntity").Single() as Entity).GetComponentsOfType<ApplicationEntity>();
            if (applicationsInPool != null)
            {
                ApplicationEntity targetApp = applicationsInPool.FirstOrDefault(a => a.ApplicationId.Equals(applicationId));
                if (targetApp != null)
                {
                    return targetApp;
                }
            }

            throw new ConfigurationException($"ApplicationDetails entity for application with id : {applicationId} is missing from ApplicationPool entity");
        }

        


        static ApplicationEntity GetApplicationDetailsEntity(IEntityManager entityManager, IComponent childComponent)
        {
            var current = childComponent;
            while (true)
            {
                if (current is IApplicationContext)
                {
                    break;
                }
                current = current.Parent;
            }

            string targetAppId = (current as IApplicationContext).GetAppContext();


            //Lookup in application pool as new application could have been added later
            var applicationsInPool = (entityManager.RootEntity.GetComponentsByTag("ApplicationPoolEntity").Single() as Entity).GetComponentsOfType<ApplicationEntity>();
            if (applicationsInPool != null)
            {
                ApplicationEntity targetApp = applicationsInPool.FirstOrDefault(a => a.ApplicationId.Equals(targetAppId));
                if (targetApp != null)
                {
                    return targetApp;
                }
            }

            throw new ConfigurationException($"ApplicationDetails entity for application with id : {targetAppId} is missing from ApplicationPool entity");
        }

    }
}
