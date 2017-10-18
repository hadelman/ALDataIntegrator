using System;
using System.Collections.Generic;
using System.Linq;
using ALDataIntegrator.RightNowService;
using ALDataIntegrator.Service.Model;

namespace ALDataIntegrator.Service
{
    internal static class RightNowServiceBaseObjectBuilder
    {
        public const int ANALYTICS_FILTER_EQUAL = 1;
        public const int ANALYTICS_FILTER_NOT_EQUAL = 2;
        public const int ANALYTICS_FILTER_LESS_THAN = 3;
        public const int ANALYTICS_FILTER_LESS_THAN_OR_EQUAL = 4;
        public const int ANALYTICS_FILTER_GREATER_THAN = 5;
        public const int ANALYTICS_FILTER_GREATER_THAN_OR_EQUAL = 6;
        public const int ANALYTICS_FILTER_LIKE = 7;
        public const int ANALYTICS_FILTER_NOT_LIKE = 8;
        public const int ANALYTICS_FILTER_RANGE = 9;
        public const int ANALYTICS_FILTER_IN_LIST = 10;
        public const int ANALYTICS_FILTER_NOT_IN_LIST = 11;
        public const int ANALYTICS_FILTER_IS_NULL = 12;
        public const int ANALYTICS_FILTER_IS_NOT_NULL = 13;
        public const int ANALYTICS_FILTER_NOT_EQUAL_OR_NULL = 14;
        public const int ANALYTICS_FILTER_NOT_LIKE_OR_NULL = 15;
        public const int ANALYTICS_FILTER_REGEX = 19;
        public const int ANALYTICS_FILTER_NOT_REGEX = 20;
        public const int ANALYTICS_FILTER_AMPERSAND = 24;

        internal static NamedID CreateNamedID(string name, long id)
        {
            var result = new NamedID
            {
                ID = new ID
                {
                    idSpecified = true,
                    id = id
                },
                Name = name
            };
            return result;
        }

        internal static NamedID CreateNamedID(long id)
        {
            var result = new NamedID
            {
                ID = new ID
                {
                    idSpecified = true,
                    id = id
                },
            };
            return result;
        }

        internal static NamedID CreateNamedID(string name)
        {
            var result = new NamedID
            {
                Name = name
            };
            return result;
        }

        internal static NamedIDDelta CreateNamedIDDelta(int id, ActionEnum action)
        {
            var result = new NamedIDDelta
                             {
                                 ID = new ID
                                          {
                                              id = id,
                                              idSpecified = true
                                          },
                                 action = action,
                                 actionSpecified = true
                             };

            return result;
        }

        internal static NamedIDHierarchy CreateNamedIDierarchy(long id)
        {
            var result = new NamedIDHierarchy
                             {
                                 ID = new ID
                                          {
                                              id = id,
                                              idSpecified = true
                                          },
                             };

            return result;
        }

        internal static IncidentContact CreateIncidentContact(long id)
        {
            var result = new IncidentContact
            {
                Contact = CreateNamedID(id)
            };

            return result;
        }

        internal static GenericObject CreateCustomFields(List<RightNowServiceGenericFieldDTO> genericFields)
        {
            // create a container for all of the packages
            var packageContainer = new List<GenericField>();

            // get a list of the unique packages
            var packagesList = (from f in genericFields select f.PackageName).Distinct().ToList();

            foreach (string package in packagesList)
            {
                // get the objects for that pacakge
                List<RightNowServiceGenericFieldDTO> packageFields =
                    genericFields.Where(x => x.PackageName == package).ToList();

                // create the generic field objects for each of them
                List<GenericField> genericFieldList =
                    packageFields.Select(
                        packageField =>
                        CreateGenericField(packageField.CustomFieldName, packageField.DataType, packageField.DataValue))
                        .ToList();

                // wrap the field list in a GenericObject
                var genericFieldListObject = new GenericObject
                                                 {
                                                     GenericFields = genericFieldList.ToArray()
                                                 };

                // create the container and put the items in it
                packageContainer.Add(CreateGenericField(package, DataTypeEnum.OBJECT, genericFieldListObject));
            }

            var result = new GenericObject
                             {
                                 GenericFields = packageContainer.ToArray()
                             };

            return result;
        }

        internal static GenericField CreateGenericField(string genericFieldName, DataTypeEnum dataType, object dataValues)
        {
            var result = new GenericField
                             {
                                 name = genericFieldName,
                                 dataType = dataType,
                                 dataTypeSpecified = true,
                                 DataValue = CreateDataValue(dataType, dataValues)
                             };

            return result;
        }

        internal static DataValue CreateDataValue(DataTypeEnum dataType, object dataValues)
        {
            var result = new DataValue();

            switch (dataType)
            {
                    // add more types as needed

                case DataTypeEnum.STRING:
                    if (!string.IsNullOrEmpty((string)dataValues))
                    {
                        result.Items = new[] { dataValues };
                        result.ItemsElementName = new[] { ItemsChoiceType.StringValue };
                    }
                    else
                    {
                        result = null;
                    }
                    break;
                case DataTypeEnum.BOOLEAN:
                    result.Items = new object[] {(bool) dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.BooleanValue};
                    break;
                case DataTypeEnum.INTEGER:
                    result.Items = new object[] {(int) dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.IntegerValue};
                    break;
                case DataTypeEnum.LONG:
                    result.Items = new object[] {(long) dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.LongValue};
                    break;
                case DataTypeEnum.OBJECT:
                    result.Items = new[] {dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.ObjectValue};
                    break;
                case DataTypeEnum.NAMED_ID:
                    result.Items = new[] {dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.NamedIDValue};
                    break;
                case DataTypeEnum.DATE:
                    result.Items = new[] {dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.DateValue};
                    break;
                case DataTypeEnum.DATETIME:
                    result.Items = new[] { dataValues };
                    result.ItemsElementName = new[] { ItemsChoiceType.DateTimeValue };
                    break;
                case DataTypeEnum.BASE64_BINARY:
                    result.Items = new[] {dataValues};
                    result.ItemsElementName = new[] {ItemsChoiceType.Base64BinaryValue};
                    break;
                default:
                    throw new NotImplementedException(string.Format("The data type {0} has not been implemented yet!",
                                                                    dataType));
            }

            return result;
        }

        internal static StatusWithType CreateStatusWithType(string statusName)
        {
            var result = new StatusWithType
                             {
                                 Status = new NamedID
                                              {
                                                  Name = statusName
                                              }
                             };

            return result;
        }

        internal static Email[] CreateEmail(string emailAddress)
        {
            return RightNowServiceBaseObjectBuilder.ProcessEmail(emailAddress, ActionEnum.add);
        }
        internal static Email[] UpdateEmail(string emailAddress)
        {
            return RightNowServiceBaseObjectBuilder.ProcessEmail(emailAddress, ActionEnum.update);
        }

        private static Email[] ProcessEmail(string emailAddress, ActionEnum actionEnum)
        {
            var result = new[]
                             {
                                 new Email
                                     {
                                         action = actionEnum,
                                         actionSpecified = true,
                                         Address = emailAddress,
                                         AddressType = CreateNamedID(0),
                                         Invalid = false,
                                         InvalidSpecified = true
                                     }
                             };

            return result;
        }

        internal static Phone[] CreatePhone(string phone, int phoneType)
        {
            return RightNowServiceBaseObjectBuilder.ProcessPhone(phone, phoneType, ActionEnum.add);
        }

        internal static Phone[] UpdatePhone(string phone, int phoneType)
        {
            return RightNowServiceBaseObjectBuilder.ProcessPhone(phone, phoneType, ActionEnum.update);
        }

        internal static Phone[] ProcessPhone(string phone, int phoneType, ActionEnum actionEnum)
        {
            var result = new[]
                             {
                                 new Phone
                                     {
                                         action = actionEnum,
                                         actionSpecified = true,
                                         Number = phone,
                                         PhoneType = CreateNamedID(phoneType),
                                     }
                             };

            return result;
        }

        internal static Phone CreateSinglePhone(string phone, int phoneType)
        {
            return RightNowServiceBaseObjectBuilder.ProcessSinglePhone(phone, phoneType, ActionEnum.add);
        }

        internal static Phone UpdateSinglePhone(string phone, int phoneType)
        {
            return RightNowServiceBaseObjectBuilder.ProcessSinglePhone(phone, phoneType, ActionEnum.update);
        }
        internal static Phone ProcessSinglePhone(string phone, int phoneType, ActionEnum actionEnum)
        {
            var result = new Phone
            {
                action = actionEnum,
                actionSpecified = true,
                Number = phone,
                PhoneType = CreateNamedID(phoneType),
            };

            return result;
        }

        internal static TypedAddress[] CreateTypedAddress(string street, string city, int stateOrProvinceID, string postalCode, int countryID, int addressTypeID)
        {
            return RightNowServiceBaseObjectBuilder.ProcessTypedAddress(street, city, stateOrProvinceID, postalCode, countryID, addressTypeID, ActionEnum.add);
        }

        internal static TypedAddress[] UpdateTypedAddress(string street, string city, int stateOrProvinceID, string postalCode, int countryID, int addressTypeID)
        {
            return RightNowServiceBaseObjectBuilder.ProcessTypedAddress(street, city, stateOrProvinceID, postalCode, countryID, addressTypeID, ActionEnum.update);
        }

        internal static TypedAddress[] ProcessTypedAddress(string street, string city, int stateOrProvinceID, string postalCode, int countryID, int addressTypeID, ActionEnum actionEnum)
        {
            var typedAddress = new TypedAddress
                        {
                            action = actionEnum,
                            actionSpecified = true,
                            Street = street,
                            City = city,
                            StateOrProvince = CreateNamedID(stateOrProvinceID),
                            PostalCode = postalCode,
                            Country = CreateNamedID(countryID),
                            AddressType = CreateNamedID(addressTypeID)
                        };
            if (stateOrProvinceID > 0)
            {
                typedAddress.StateOrProvince = CreateNamedID(stateOrProvinceID);
            }
            if (countryID > 0)
            {
                typedAddress.Country = CreateNamedID(countryID);
            }

            return new[] { typedAddress };
        }

        // refactored from http://communities.rightnow.com/posts/3b4e1e1ae6
        internal static GenericField CreateAttachmentText(AttachmentData data)
        {
            return CreateAttachment(data, "text/plain");
        }
        internal static GenericField CreateAttachmentZip(AttachmentData data)
        {
            return CreateAttachment(data, "application/x-zip-compressed");
        }

        internal static GenericField CreateAttachment(AttachmentData data, string contentType)
        {
            var gfs = new List<GenericField>
            {
                CreateGenericField("ContentType", DataTypeEnum.STRING, contentType), // "text/plain", "application/x-zip-compressed"
                CreateGenericField("FileName", DataTypeEnum.STRING, data.AttachmentName),
                CreateGenericField("Data", DataTypeEnum.BASE64_BINARY, data.AttachmentContent)
            };

            // add a file attachment
            var result = new GenericField
            {
                name = "FileAttachments",
                DataValue = new DataValue
                {
                    ItemsElementName = new [] { ItemsChoiceType.ObjectValueList },
                    Items = new []
                    {
                        new GenericObject() 
                        {
                            ObjectType = new RNObjectType() { TypeName = "FileAttachment" },
                            GenericFields = gfs.ToArray()
                        }
                    }
                }
            };

            return result;
        }

        #region AnalyticsReportFilter Operations

        internal static AnalyticsReportFilter GetDateTimeRangeAnalyticsReportFilter(string Name, DateTime fromDateTime, DateTime toDateTime)
        {
            return GetDateTimeRangeAnalyticsReportFilter(Name, fromDateTime, toDateTime, false, false, false, true);
        }

        internal static AnalyticsReportFilter GetDateTimeRangeAnalyticsReportFilter(string Name, DateTime fromDateTime, DateTime toDateTime, bool specifyAttributes, bool specifyDatatype, bool specifyOperator, bool appendEndingZ)
        {
            //Assigning the filter as defined on the RightNow Analytics Report 
            AnalyticsReportFilter filter = new AnalyticsReportFilter();

            if (specifyAttributes)
            {
                AnalyticsReportFilterAttributes attribute = new AnalyticsReportFilterAttributes();
                attribute.Editable = false;
                attribute.Required = false;
                filter.Attributes = attribute;
            }

            if (specifyDatatype)
            {
                NamedID datatype = new NamedID();
                datatype.Name = "Date Time";
                filter.DataType = datatype;
            }

            filter.Name = Name;
            if (fromDateTime > DateTime.MinValue && toDateTime > DateTime.MinValue)
            {
                if (specifyDatatype)
                    filter.Operator = new NamedID { ID = new ID { id = ANALYTICS_FILTER_RANGE, idSpecified = true } };
                if (appendEndingZ)
                    filter.Values = new[] { fromDateTime.ToString("s") + "Z", toDateTime.ToString("s") + "Z" };
                else
                    filter.Values = new[] { fromDateTime.ToString("s"), toDateTime.ToString("s") };
            }
            else
            {
                if (fromDateTime > DateTime.MinValue)
                {
                    if (specifyDatatype)
                        filter.Operator = new NamedID { ID = new ID { id = ANALYTICS_FILTER_GREATER_THAN_OR_EQUAL, idSpecified = true } };
                    if (appendEndingZ)
                        filter.Values = new[] { fromDateTime.ToString("s") + "Z" };
                    else
                        filter.Values = new[] { fromDateTime.ToString("s") };
                }
                else if (toDateTime > DateTime.MinValue)
                {
                    if (specifyDatatype)
                        filter.Operator = new NamedID { ID = new ID { id = ANALYTICS_FILTER_LESS_THAN_OR_EQUAL, idSpecified = true } };
                    if (appendEndingZ)
                        filter.Values = new[] { toDateTime.ToString("s") + "Z" };
                    else
                        filter.Values = new[] { toDateTime.ToString("s") };
                }
            }

            return filter;
        }

        internal static AnalyticsReportFilter GetStringEqualAnalyticsReportFilter(string Name, String searchPhrase)
        {
            return GetStringAnalyticsReportFilter(Name, searchPhrase, ANALYTICS_FILTER_EQUAL, false, false, false);
        }

        internal static AnalyticsReportFilter GetStringEqualAnalyticsReportFilter(string Name, String searchPhrase, int searchOperator, bool specifyAttributes, bool specifyDatatype, bool specifyOperator)
        {
            return GetStringAnalyticsReportFilter(Name, searchPhrase, ANALYTICS_FILTER_EQUAL, specifyAttributes, specifyDatatype, specifyOperator);
        }

        internal static AnalyticsReportFilter GetStringLikeAnalyticsReportFilter(string Name, String searchPhrase)
        {
            return GetStringAnalyticsReportFilter(Name, searchPhrase, ANALYTICS_FILTER_LIKE, false, false, false);
        }

        internal static AnalyticsReportFilter GetStringLikeAnalyticsReportFilter(string Name, String searchPhrase, int searchOperator, bool specifyAttributes, bool specifyDatatype, bool specifyOperator)
        {
            return GetStringAnalyticsReportFilter(Name, searchPhrase, ANALYTICS_FILTER_LIKE, specifyAttributes, specifyDatatype, specifyOperator);
        }

        internal static AnalyticsReportFilter GetStringInListAnalyticsReportFilter(string Name, String searchPhrase)
        {
            return GetStringAnalyticsReportFilter(Name, searchPhrase, ANALYTICS_FILTER_IN_LIST, false, false, false);
        }

        internal static AnalyticsReportFilter GetStringInListAnalyticsReportFilter(string Name, String searchPhrase, int searchOperator, bool specifyAttributes, bool specifyDatatype, bool specifyOperator)
        {
            return GetStringAnalyticsReportFilter(Name, searchPhrase, ANALYTICS_FILTER_IN_LIST, specifyAttributes, specifyDatatype, specifyOperator);
        }

        internal static AnalyticsReportFilter GetStringAnalyticsReportFilter(string Name, String searchPhrase, int searchOperator, bool specifyAttributes, bool specifyDatatype, bool specifyOperator)
        {
            //Assigning the filter as defined on the RightNow Analytics Report 
            AnalyticsReportFilter filter = new AnalyticsReportFilter();

            if (specifyAttributes)
            {
                AnalyticsReportFilterAttributes attribute = new AnalyticsReportFilterAttributes();
                attribute.Editable = false;
                attribute.Required = false;
                filter.Attributes = attribute;
            }

            if (specifyDatatype)
            {
                NamedID datatype = new NamedID();
                datatype.Name = Name.GetType().ToString();
                filter.DataType = datatype;
            }

            filter.Name = Name;
            if (specifyOperator)
                filter.Operator = new NamedID { ID = new ID { id = searchOperator, idSpecified = true } };
            filter.Values = new String[] { searchPhrase };

            return filter;
        }

        #endregion

        #region Generic Object Getters

        internal static object GetGenericObjectFieldValue(string fieldName, GenericObject genericObject)
        {
            object fieldValue = null;

            if (genericObject != null && genericObject.GenericFields != null)
            {
                foreach (GenericField genericField in genericObject.GenericFields)
                {
                    if (genericField.name == fieldName)
                    {
                        fieldValue = genericField.DataValue.Items[0];
                        break;
                    }
                }
            }
            return fieldValue;
        }
        
        #endregion

    }
}
