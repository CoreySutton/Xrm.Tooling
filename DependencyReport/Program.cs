using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString = Properties.Settings.Default.CrmConnectionString;
                using (OrganizationServiceProxy orgSvc = CrmConnectorUtil.Connect(connectionString) as OrganizationServiceProxy)
                {
                    // This statement is required to enable early-bound type support.
                    orgSvc.EnableProxyTypes();

                    FunctionType functionType = Prompter.ChooseFunction();
                    switch (functionType)
                    {
                        case FunctionType.ShowSolutionDependencies:
                            {
                                new SolutionComponentDependencyReport(orgSvc).Create();
                                break;
                            }
                        case FunctionType.MarkAttributesAsDepricated:
                            {
                                var reporter = new AttributeNoDependencyReport(orgSvc);
                                IList<ComponentInfo> componentInfos = reporter.Find();

                                bool prompt = Prompter.YesNo("Prompt before marking depricated", true);
                                var depricator = new AttributeDeprecator(orgSvc, prompt);
                                depricator.Process(componentInfos);

                                break;
                            }
                        case FunctionType.ShowAttributesWithNoDependencies:
                            {
                                var reporter = new AttributeNoDependencyReport(orgSvc);
                                IList<ComponentInfo> componentInfos = reporter.Find();
                                break;
                            }
                        case FunctionType.ShowGlobalOptionSetDependecies:
                            {
                                new GlobalOptionSetDependecyReport(orgSvc).Create();
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException($"Function type {functionType} is not implemented");
                            }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                Console.WriteLine("Message: {0}", ex.Detail.Message);
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                Console.WriteLine("Inner Fault: {0}", ex.InnerException.Message ?? "No Inner Fault");
            }
            catch (Exception ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);

                    FaultException<OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<OrganizationServiceFault>;
                    if (fe != null)
                    {
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                        Console.WriteLine("Message: {0}", fe.Detail.Message);
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText);
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.
            finally
            {
                Console.WriteLine("Press <Enter> to exit.");
                Console.ReadLine();
            }
        }
    }
}
