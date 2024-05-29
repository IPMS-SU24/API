#!/bin/bash
set -e

AWS_REGION="ap-southeast-1"

SECRET_NAME="env"

SECRET_VALUE=$(aws secretsmanager get-secret-value --region $AWS_REGION --secret-id $SECRET_NAME --query SecretString --output text)

echo "SECRET_VALUE: $SECRET_VALUE"

# Use process substitution instead of a pipe
while IFS="=" read -r key value; do
    # Trim whitespace from key and value
    key=$(echo "$key" | awk '{$1=$1};1')
    value=$(echo "$value" | awk '{$1=$1};1')
    echo "$key - $value"
    echo "export $key='$value'" >> ~/.bashrc
    echo "export $key='$value'" >> ~/.bash_profile
done < <(echo "$SECRET_VALUE" | jq -r 'to_entries[] | "\(.key)=\(.value)"')

source ~/.bash_profile
