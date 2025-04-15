# EvilGiraf Frontend Guide

This guide provides information about the EvilGiraf web interface, its features, and how to use them effectively.

## Overview

The EvilGiraf frontend is built with:

- React
- TypeScript
- Tailwind CSS
- Vite

## Features

### Application Management

#### Application List

- View all applications in a table format
- Create new applications
- Access application details

#### Application Details

- Detailed view of each application
- Configuration settings
- Deployment status

## Getting Started

1. Access the web interface:

   [evilgiraf.pyxis.dopolytech.fr](https://evilgiraf.pyxis.dopolytech.fr)

2. Log in with your api key

3. Navigate the dashboard:
   - View application overview
   - Create your application
   - Access application details
   - Deploy it

## Using the Interface

### Creating a New Application

1. Click "New Application" button
2. Fill in the required fields:
   - Name
   - Type
      (Docker for a docker image, Git for a repository)
   - Link
   - Version
      (optional, image tag for docker, branch/tag name/commit id for git. defaults to latest or main)
3. Configure additional settings
4. Click "Create"

### Managing Applications

#### View Details

1. Click on an application in the list
2. View detailed information
3. Access configuration options
4. Monitor deployment status

#### Edit Application

1. Open application details
2. Click "Edit" button
3. Modify required fields
4. Save changes

#### Delete Application

1. Select application(s)
2. Click "Delete" button
3. Confirm deletion

#### Deploy/Redeploy Application

1. Open application details
2. Click "Deploy"/"Redeploy" button
3. Monitor deployment status

#### Undeploy Application

1. Open application details
2. Click "Undeploy" button

## Troubleshooting

### Common Issues

1. Page Not Loading
   - Check internet connection
   - Clear browser cache
   - Try different browser

2. Actions Not Working
   - Verify permissions
   - Check input validity
   - Refresh page

3. Performance Issues
   - Reduce data load
   - Clear browser cache
   - Check network connection

### Getting Help

- Check documentation
- Contact support
- Submit bug reports

## Mobile Support

- Responsive design
- Touch-friendly interface
