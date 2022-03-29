import * as pulumi from "@pulumi/pulumi";
import * as aws from "@pulumi/aws";

const role = new aws.iam.Role('webadvertapi_Role',
    {
        assumeRolePolicy: aws.iam.assumeRolePolicyForPrincipal({
            Service: "lambda.amazonaws.com"
        })
    })

const lambdaPolicyAttachment = new aws.iam.PolicyAttachment('webadvertapi_LambdaPolicyAttachment', {
    roles: [role.name],
    policyArn: aws.iam.ManagedPolicies.AWSLambdaBasicExecutionRole
})

const dynamoDbPolicyAttachment = new aws.iam.PolicyAttachment('webadvertapi_DynamoDBPolicyAttachment', {
    roles: [role.name],
    policyArn: aws.iam.ManagedPolicies.AmazonDynamoDBFullAccess
})

const lambdaFunction = new aws.lambda.Function('webadvertapi_Lambda', {
    role: role.arn,
    handler: "WebAdvert.Api::WebAdvert.Api.LambdaEntryPoint::FunctionHandlerAsync",
    runtime: aws.lambda.DotnetCore3d1Runtime,
    timeout: 15,
    publish: false,
    code: new pulumi.asset.FileArchive("netcoreapp3.1.zip"),
    memorySize: 512
})

const httpApiGateway = new aws.apigatewayv2.Api("webadvertapi_ApiGateway", {
    protocolType:"HTTP",
    routeSelectionExpression:"${request.method} ${request.path}"
})

const httpApiGateway_LambdaIntegration = new aws.apigatewayv2.Integration("webadvertapi_ApiGatewayIntegration", {
    apiId:httpApiGateway.id,
    integrationType: "AWS_PROXY",
    integrationUri: lambdaFunction.arn,
    payloadFormatVersion: "2.0",
    timeoutMilliseconds: 30000
})

const httpApiGatewayRoute = new aws.apigatewayv2.Route("webadvertapi_ApiGatewayRoute", {
    apiId:httpApiGateway.id,
    routeKey: "ANY /{proxy+}",
    target: httpApiGateway_LambdaIntegration.id.apply(id => "integrations/"+id),
})

const httpApiGatewayStage = new aws.apigatewayv2.Stage("webadvertapi_ApiGatewayStage", {
    apiId:httpApiGateway.id,
    autoDeploy: true,
    name: "$default"
})

const lambdaPermissionsForApiGateway = new aws.lambda.Permission("webadvertapi_LambdaPermission", {
    action: "lambda:InvokeFunction",
    function: lambdaFunction.name,
    principal: "apigateway.amazonaws.com",
    sourceArn: httpApiGateway.executionArn.apply(arn => arn + "/*")
})

const advertsDynamoDbTable = new aws.dynamodb.Table("Adverts", {
    attributes: [
        { name: "Id", type: "S" },
    ],
    hashKey: "Id",
    readCapacity: 1,
    writeCapacity: 1,
    name: "Adverts"
});

const bucket = new aws.s3.Bucket("fe-webadvertapi", {
    acl: "private",
    bucket: "fe-webadvertapi",
    tags: {
        Name: "fe-webadvertapi",
    },
});

httpApiGateway.apiEndpoint.apply(endpoint => endpoint + "/adverts/v1")

const webAdvertApiSns = new aws.sns.Topic("WebAdvertApi", {});