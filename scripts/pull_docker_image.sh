#!/bin/bash
AWS_REGION="ap-southeast-1"
AWS_ACCOUNT_ID="905418022082"
REPOSITORY_NAME="ipmg_api"

# Pull docker images from ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME

# Docker images 
DOCKER_IMAGE="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:latest"

# Pull Docker image
docker pull $DOCKER_IMAGE

# Check pull docker images status
if [ $? -eq 0 ]; then
  echo "Pull Docker image thành công: $DOCKER_IMAGE"
else
  echo "Lỗi trong quá trình pull Docker image: $DOCKER_IMAGE"
  exit 1
fi

# End script
exit 0

