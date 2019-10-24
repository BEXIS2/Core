using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

using BExIS.IO.Transform.Output;

using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class MessageModel
    {
        public bool hasMessage { get; set; }
        public string Message { get; set; }
        public string CssId { get; set; }
        

        public MessageModel()
        {
            this.hasMessage = false;
            this.Message = "";
            this.CssId = "";
        }

        public static MessageModel validateDataStructureName(long Id, string Name, string cssId = "")
        {
            if (Name.Trim() == "" || string.IsNullOrEmpty(Name))
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "The name field is required.",
                    CssId = cssId
                };
            }
            else
            {
                DataStructureManager dataStructureManager = null;
                try
                {
                    dataStructureManager = new DataStructureManager();
                    List<DataStructure> dataStructures = dataStructureManager.AllTypesDataStructureRepo.Get().ToList();

                    foreach (DataStructure ds in dataStructures)
                    {
                        if (Id != ds.Id)
                        {
                            if (ds.Name.Trim().ToLower() == Name.Trim().ToLower())
                            {
                                return new MessageModel()
                                {
                                    hasMessage = true,
                                    Message = "A data structure with the same name already exists.",
                                    CssId = cssId
                                };
                            }
                        }
                    }
                }
                finally
                {
                    dataStructureManager.Dispose();
                }
            }
            return new MessageModel() { CssId = cssId };
        }

        public static MessageModel validateDataStructureDelete(long Id)
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(Id);
                return validateDataStructureDelete(Id, dataStructure);
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        public static MessageModel validateDataStructureDelete(long Id , DataStructure dataStructure)
        {
            if (dataStructure != null && dataStructure.Id != 0)
            {
                
                if (dataStructure.Datasets.Count == 0)
                {
                    return new MessageModel()
                    {
                        hasMessage = false,
                        Message = "Are you sure you want to delete the data structure " + dataStructure.Name + " (" + dataStructure.Id + ").",
                        CssId = dataStructure.Id.ToString()
                    };
                }
                else
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Cannot delete the data structure " + dataStructure.Name + " (" + dataStructure.Id + ").",
                            CssId = dataStructure.Id.ToString()
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with data structure " + Id,
                            CssId = "0"
                        };
                    }
                }
            }
            else
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "Something is wrong with data structure " + Id,
                    CssId = "0"
                };
            }
        }

        public static MessageModel validateDataStructureInUse(long Id)
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(Id);
                return validateDataStructureInUse(Id, dataStructure);
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        public static MessageModel validateDataStructureInUse(long Id, DataStructure dataStructure)
        {
            if (dataStructure != null && dataStructure.Id != 0)
            {
                DatasetManager datasetManager = null;
                bool inUse = false;
                try
                {
                    datasetManager = new DatasetManager();
                    foreach (Dataset d in dataStructure.Datasets)
                    {
                        if (datasetManager.RowCount(d.Id, null) > 0)
                        {
                            inUse = true;
                            break;
                        }
                    }
                }
                finally
                {
                    datasetManager.Dispose();
                }
                if (inUse)
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't save data structure " + dataStructure.Name + " (" + Id + "), it's uesed by a Dataset.",
                            CssId = "inUse"
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with data structure " + Id,
                            CssId = "0"
                        };
                    }
                }
                else
                {
                    return new MessageModel();
                }
            }
            else
            {
                if (Id == 0)
                {
                    return new MessageModel();
                }
                else
                {
                    return new MessageModel()
                    {
                        hasMessage = true,
                        Message = "Can't store variable for the data structure " + Id + ", it's uesed by a dataset.",
                        CssId = "0"
                    };
                }
            }
            
        }

        public static MessageModel validateAttributeDelete(long Id)
        {
            DataContainerManager dataContainerManager = null;
            try
            {
                dataContainerManager = new DataContainerManager();
                DataAttribute dataAttribute = dataContainerManager.DataAttributeRepo.Get(Id);
                return validateAttributeDelete(Id, dataAttribute);
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        public static MessageModel validateAttributeDelete(long Id, DataAttribute dataAttribute)
        {
            if (dataAttribute != null && dataAttribute.Id != 0)
            {

                if (dataAttribute.UsagesAsVariable.Count == 0)
                {
                    return new MessageModel()
                    {
                        hasMessage = false,
                        Message = "Are you sure you want to delete the variable temlate " + dataAttribute.Name + " (" + dataAttribute.Id + ").",
                        CssId = dataAttribute.Id.ToString()
                    };
                }
                else
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't delete the variable temlate " + dataAttribute.Name + " (" + dataAttribute.Id + ").",
                            CssId = dataAttribute.Id.ToString()
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with variable temlate " + Id,
                            CssId = "0"
                        };
                    }
                }
            }
            else
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "Something is wrong with variable temlate " + Id,
                    CssId = "0"
                };
            }
        }

        public static MessageModel validateAttributeName(long Id, string Name, string cssId = "")
        {
            if (Name.Trim() == "" || string.IsNullOrEmpty(Name))
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "The name field is required.",
                    CssId = cssId
                };
            }
            else
            {
                DataContainerManager dataContainerManager = null;
                dataContainerManager = new DataContainerManager();
                try
                {
                    List<DataAttribute> dataAttributes = dataContainerManager.DataAttributeRepo.Get().ToList();

                    foreach (DataAttribute da in dataAttributes)
                    {
                        if (Id != da.Id)
                        {
                            if (da.Name.Trim().ToLower() == Name.Trim().ToLower())
                            {
                                return new MessageModel()
                                {
                                    hasMessage = true,
                                    Message = "A variable template with same name already exists.",
                                    CssId = cssId
                                };
                            }
                        }
                    }
                }
                finally
                {
                    dataContainerManager.Dispose();
                }
            }
            return new MessageModel() { CssId = cssId };
        }

        public static MessageModel validateAttributeInUse(long Id)
        {
            DataContainerManager dataAttributeManager = null;
            try
            {
                dataAttributeManager = new DataContainerManager();
                DataAttribute dataAttribute = dataAttributeManager.DataAttributeRepo.Get(Id);
                return validateAttributeInUse(Id, dataAttribute);
            }
            finally
            {
                dataAttributeManager.Dispose();
            }
        }

        public static MessageModel validateAttributeInUse(long Id, DataAttribute dataAttribute)
        {
            if (dataAttribute != null && dataAttribute.Id != 0)
            {
                if (dataAttribute.UsagesAsVariable.Count > 0)
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't save variable template " + dataAttribute.Name + " (" + Id + "), it's uesed by a data structure.",
                            CssId = "inUse"
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with variable template " + Id,
                            CssId = "0"
                        };
                    }
                }
                else
                {
                    return new MessageModel();
                }
            }
            else
            {
                if (Id == 0)
                {
                    return new MessageModel();
                }
                else
                {
                    return new MessageModel()
                    {
                        hasMessage = true,
                        Message = "Can't save variable template " + Id + ", it's uesed by a data structure.",
                        CssId = "0"
                    };
                }
            }

        }
    }
}

    