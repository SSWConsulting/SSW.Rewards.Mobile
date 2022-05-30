# Deployment

### Web API / Infrastructure

1. Merge PR into master
   ![image.png](imgs/deployment-merge.png)
   **Figure: Merge Pull Request after getting approval**

1. Build pipeline will automatically run and deploy the changes into the DEV environment
   ![image.png](imgs/deployment-successful-build.png)
   **Figure: Wait for a successful build**

1. Get approval on the Production release to deploy to Production

### Mobile App

1. Merge PR into master
1. Build pipelines for both Android and iOS will run and push the changes to App Center testers
1. Get approval on the Production release to deploy to Play Store and Production
