﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.OleDb;
using Repository.DAL;
using Procurement.Controllers;
using System.Reflection;
using StaticClasses;

namespace Procurement
{
    public partial class FrmBOM : Form
    {
        ProjectController _pc;
        BOMController _bc;
        List<Project> _LstProjects;
        ProjectEmployeeDetailController _pedc;
        DataTable _dtProjects;
        DataTable _dtSalesBOM;
        DataTable _dtDesignBOM;
        DataTable _dtActualBOM;
        decimal _projectCode;
        bool _newMode;
        Project _currentLoadedProject;
        public FrmBOM()
        {
            InitializeComponent();
        }

        private void FrmBOM_Load(object sender, EventArgs e)
        {
            try
            {
                if (LoginInfo.LoginEmployee.EmployeeTypeCode == Constants.EMPLOYEE)
                {
                    txtProjectName.Enabled = false;
                    txtProjectCustomerName.Enabled = false;
                    txtProjectEndUser.Enabled = false;
                        
                    btnNewProject.Enabled = false;
                    btnLoadBOM.Enabled = false;
                }
                
                _pc = new ProjectController();
                _LstProjects = _pc.GetModels();
                //insert into the list

                //list.Where(x => x.BOMTypeCode == 1).Select(x => new
                //{
                //    x.SORef,
                //    x.SerialNo,
                //    x.ProductCategory,
                //    x.Product,
                //    x.CostHead,
                //    x.CostSubHead,
                //    x.System,
                //    x.Area,
                //    x.Panel,
                //    x.Category,
                //    x.Manufacturer,
                //    x.PartNo,
                //    x.Description,
                //    x.Qty,
                //    x.UnitCost,
                //    x.ExCost,
                //    x.UnitPrice,
                //    x.ExPrice
                //});


                _dtProjects = ToDataTable<Project>(_LstProjects);
                _dtProjects.Columns.Remove("BOMs");
                _dtProjects.Columns.Remove("ProjectEmployeeDetails");

                DataView dv = _dtProjects.DefaultView;
                dv.Sort = "ProjectCode desc";
                _dtProjects = dv.ToTable();

                dataGridViewProjects.DataSource = _dtProjects;

                if (_LstProjects.Count == 0)
                {
                    
                    _newMode = true;
                    //_pc.ReseedProjectPk();
                    //txtProjectCode.Text = (1).ToString();
                    ClearAll();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }


        private void LoadBOM_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg_im = new OpenFileDialog();
                dlg_im.Filter = "Excel File|*.xls;*.xlsx;*.xlsm";
                //dlg_im.Filter = "Excel File|*.xlsx";

                if (dlg_im.ShowDialog() == DialogResult.OK)
                {
                    //dataGridView1.Rows.Clear();
                    txtBOMFilePath.Text = dlg_im.FileName;


                    string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + txtBOMFilePath.Text + ";Extended Properties='Excel 12.0 XML;HDR=YES;';";

                    OleDbConnection Con = new OleDbConnection(constr);

                    Con.Open();

                    // Get the name of the first worksheet:
                    DataTable dbSchema = Con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dbSchema == null || dbSchema.Rows.Count < 1)
                    {
                        throw new Exception("Error: Could not determine the name of the first worksheet.");
                    }
                    string firstSheetName = dbSchema.Rows[0]["TABLE_NAME"].ToString();



                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + firstSheetName + "]", Con);



                    OleDbDataAdapter sda = new OleDbDataAdapter(cmd);
                    DataTable dtTemp = new DataTable();
                    sda.Fill(dtTemp);
                    Con.Close();
                    //DataRow newDataRow;
                    //////////////////////////////////////////////////////////////
                    _dtSalesBOM = new DataTable();
                    //foreach (DataColumn dc in dtTemp.Columns)
                    //{
                    _dtSalesBOM = dtTemp.Clone();
                    //}

                    foreach (DataRow dr in dtTemp.Rows)
                    {
                        //string colName=gvr.Cells[0].OwningColumn.HeaderText;

                        bool isAdd = false;
                        for (int i = 0; i < dtTemp.Columns.Count; i++)
                        {
                            //if (dr[i] == null || dr[i] == DBNull.Value || String.IsNullOrWhiteSpace(dr[i].ToString()))
                            if (dr[i] == DBNull.Value)
                            {
                                isAdd = false;
                            }
                            else
                            {
                                isAdd = true;
                                break;
                            }
                        }

                        if (isAdd == true)
                        {
                            //can not add like this _dtSalesBOM.Rows.Add(dr); :(  have to add new row and then add to list
                            //newDataRow = _dtSalesBOM.NewRow();
                            //for (int i = 0; i <  dtTemp.Columns.Count; i++)
                            //{
                            //    newDataRow[i] = dr[i];

                            //}
                            //_dtSalesBOM.Rows.Add(newDataRow);

                            _dtSalesBOM.Rows.Add(dr.ItemArray);
                        }

                    }
                    dataGridView1.DataSource = _dtSalesBOM;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }



        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            //mnuCopyAllToDesignBOM.ShowDropDown();
            //contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                MenuStripSaleBOM.Show(Cursor.Position);
            }
        }

        private void itmCopyAllToDesignBOM_Click(object sender, EventArgs e)
        {
            _dtDesignBOM = new DataTable();
            _dtDesignBOM = _dtSalesBOM.Copy();

            //dataGridView2.DataSource =dataGridView1.DataSource;
            dataGridView2.DataSource = _dtDesignBOM;
            tabControl1.SelectedTab = tabDesignBOM;

        }
        private void dataGridView2_MouseClick(object sender, MouseEventArgs e)
        {
            //mnuCopyAllToDesignBOM.ShowDropDown();
            //contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                MenuStripDesignBOM.Show(Cursor.Position);
            }
        }
        private void itmCopyAllToActualBOM_Click(object sender, EventArgs e)
        {
            _dtActualBOM = new DataTable();
            _dtActualBOM = _dtDesignBOM.Copy();

            //dataGridView2.DataSource =dataGridView1.DataSource;
            dataGridView3.DataSource = _dtActualBOM;
            tabControl1.SelectedTab = tabActualBOM;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            Project projModel;
            ProjectEmployeeDetail ped;
            if (_newMode == true)
            {
                //SaveData();
                projModel = FillProjectModel();
                projModel.CreatedBy = LoginInfo.LoginEmployee.EmployeeCode;
                projModel.CreatedDate = DateTime.Now;
                _pc = new ProjectController(projModel);
                _pc.Save();
                //------------------


                DataRow NewRow = _dtProjects.NewRow();
                NewRow[0] = projModel.ProjectCode;//decimal.Parse(txtProjectCode.Text);
                NewRow[1] = projModel.ProjectName;//txtProjectName.Text;
                NewRow[2] = projModel.Customer;//txtProjectCustomerName.Text;
                NewRow[3] = projModel.EndUser;//txtProjectEndUser.Text;

                _dtProjects.Rows.Add(NewRow);

                DataView dv = _dtProjects.DefaultView;
                dv.Sort = "ProjectCode desc";
                _dtProjects = dv.ToTable();

                dataGridViewProjects.DataSource = _dtProjects;
                _LstProjects.Add(projModel);

                //------------------
                
                ped = new ProjectEmployeeDetail();
                ped.EmployeeCode = LoginInfo.LoginEmployee.EmployeeCode;//_EmployeeCode;
                ped.ProjectCode = projModel.ProjectCode;//(decimal)row1["ProjectCode"];

                _pedc = new ProjectEmployeeDetailController(ped);
                _pedc.Save();
                //-------------------


                _newMode = false;
            }
            else
            {
                //UpdateData();
                projModel = FillProjectModel();
                projModel.CreatedBy = _currentLoadedProject.CreatedBy;
                projModel.CreatedDate = _currentLoadedProject.CreatedDate;
                projModel.UpdatedBy = LoginInfo.LoginEmployee.EmployeeCode;
                projModel.UpdateDate = DateTime.Now;
                _pc = new ProjectController(projModel);
                _pc.UpdateModel(projModel);
                Project proj = _LstProjects.Where(x => x.ProjectCode == projModel.ProjectCode).FirstOrDefault();
                _LstProjects.Remove(proj);
                _LstProjects.Add(projModel);

            }

            List<BOM> LstObjBom;
            LstObjBom = FillBOMModel1(ref projModel);
            _bc = new BOMController(LstObjBom);
            _bc.SaveList(projModel.ProjectCode, 1);
            //---
            LstObjBom = FillBOMModel2(ref projModel);
            _bc = new BOMController(LstObjBom);
            _bc.SaveList(projModel.ProjectCode, 2);
            //---
            LstObjBom = FillBOMModel3(ref projModel);
            _bc = new BOMController(LstObjBom);
            _bc.SaveList(projModel.ProjectCode, 3);

            //Project proj = _LstProjects.Where(x => x.ProjectCode == projModel.ProjectCode).FirstOrDefault();

            //if (proj == null)
            //{
            //    _LstProjects.Add(projModel);
            //}
            //else
            //{
            //    _LstProjects.Remove(proj);
            //    _LstProjects.Add(projModel);
            //}


            //_LstProjects
            this.Enabled = true;

           
        }
      
        private List<BOM> FillBOMModel1(ref Project pProjectModel)
        {
            List<BOM> LstObjBom = new List<BOM>();
            foreach (DataGridViewRow gvr in dataGridView1.Rows)
            {
                //string colName=gvr.Cells[0].OwningColumn.HeaderText;

                bool isAdd = false;
                for (int i = 0; i < gvr.Cells.Count; i++)
                {
                    //if (gvr.Cells[i].Value == null || gvr.Cells[i].Value == DBNull.Value || String.IsNullOrWhiteSpace(gvr.Cells[i].Value.ToString()))
                        if (gvr.Cells[i].Value == null)
                        {
                        isAdd = false;
                    }
                    else
                    {
                        isAdd = true;
                        break;
                    }
                }

                //if (lObjBom.SORef != null && lObjBom.SerialNo != null && lObjBom.ProductCategory != null && lObjBom.Product != null && lObjBom.CostHead != null && lObjBom.CostSubHead != null
                //     && lObjBom.System != null && lObjBom.Area != null && lObjBom.Panel != null && lObjBom.Description != null && lObjBom.Qty != null && lObjBom.UnitCost != null
                //      && lObjBom.ExCost != null && lObjBom.UnitPrice != null && lObjBom.ExPrice != null)
                //if (!(string.IsNullOrEmpty( gvr.Cells[0].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[1].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[2].Value.ToString())
                //    && string.IsNullOrEmpty(gvr.Cells[3].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[4].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[5].Value.ToString())
                //    && string.IsNullOrEmpty(gvr.Cells[6].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[7].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[8].Value.ToString())
                //    && string.IsNullOrEmpty(gvr.Cells[9].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[10].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[11].Value.ToString())
                //    && string.IsNullOrEmpty(gvr.Cells[12].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[13].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[14].Value.ToString())
                //    && string.IsNullOrEmpty(gvr.Cells[15].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[16].Value.ToString()) && string.IsNullOrEmpty(gvr.Cells[17].Value.ToString())))

                if (isAdd == true)
                {
                    BOM lObjBom = new BOM();
                    //lObjBom.BOMCode = (string)gvr.Cells[0].Value;
                    lObjBom.BOMTypeCode = 1;//(string)gvr.Cells[0].Value;
                    lObjBom.ProjectCode = pProjectModel.ProjectCode;

                    lObjBom.SORef = gvr.Cells[0].Value.ToString();
                    lObjBom.SerialNo = gvr.Cells[1].Value.ToString();
                    lObjBom.ProductCategory = gvr.Cells[2].Value.ToString();
                    lObjBom.Product = gvr.Cells[3].Value.ToString();
                    lObjBom.CostHead = gvr.Cells[4].Value.ToString();
                    lObjBom.CostSubHead = gvr.Cells[5].Value.ToString();
                    lObjBom.System = gvr.Cells[6].Value.ToString();
                    lObjBom.Area = gvr.Cells[7].Value.ToString();
                    lObjBom.Panel = gvr.Cells[8].Value.ToString();
                    lObjBom.Category = gvr.Cells[9].Value.ToString();
                    lObjBom.Manufacturer = gvr.Cells[10].Value.ToString();
                    lObjBom.PartNo = gvr.Cells[11].Value.ToString();
                    lObjBom.Description = gvr.Cells[12].Value.ToString();
                    lObjBom.Qty = gvr.Cells[13].Value.ToString();
                    lObjBom.UnitCost = gvr.Cells[14].Value.ToString();
                    lObjBom.ExCost = gvr.Cells[15].Value.ToString();
                    lObjBom.UnitPrice = gvr.Cells[16].Value.ToString();
                    lObjBom.ExPrice = gvr.Cells[17].Value.ToString();

                    LstObjBom.Add(lObjBom);
                    pProjectModel.BOMs.Add(lObjBom);

                }


            }
            return LstObjBom;

        }

        private List<BOM> FillBOMModel2(ref Project pProjectModel)
        {
            List<BOM> LstObjBom = new List<BOM>();
            foreach (DataGridViewRow gvr in dataGridView2.Rows)
            {
                //string colName=gvr.Cells[0].OwningColumn.HeaderText;

                bool isAdd = false;
                for (int i = 0; i < gvr.Cells.Count; i++)
                {
                    //if (gvr.Cells[i].Value == null || gvr.Cells[i].Value == DBNull.Value || String.IsNullOrWhiteSpace(gvr.Cells[i].Value.ToString()))
                        if (gvr.Cells[i].Value == null)
                        {
                        isAdd = false;
                    }
                    else
                    {
                        isAdd = true;
                        break;
                    }
                }


                if (isAdd == true)
                {
                    BOM lObjBom = new BOM();
                    //lObjBom.BOMCode = (string)gvr.Cells[0].Value;
                    lObjBom.BOMTypeCode = 2;//(string)gvr.Cells[0].Value;
                    lObjBom.ProjectCode = pProjectModel.ProjectCode;

                    lObjBom.SORef = gvr.Cells[0].Value.ToString();
                    lObjBom.SerialNo = gvr.Cells[1].Value.ToString();
                    lObjBom.ProductCategory = gvr.Cells[2].Value.ToString();
                    lObjBom.Product = gvr.Cells[3].Value.ToString();
                    lObjBom.CostHead = gvr.Cells[4].Value.ToString();
                    lObjBom.CostSubHead = gvr.Cells[5].Value.ToString();
                    lObjBom.System = gvr.Cells[6].Value.ToString();
                    lObjBom.Area = gvr.Cells[7].Value.ToString();
                    lObjBom.Panel = gvr.Cells[8].Value.ToString();
                    lObjBom.Category = gvr.Cells[9].Value.ToString();
                    lObjBom.Manufacturer = gvr.Cells[10].Value.ToString();
                    lObjBom.PartNo = gvr.Cells[11].Value.ToString();
                    lObjBom.Description = gvr.Cells[12].Value.ToString();
                    lObjBom.Qty = gvr.Cells[13].Value.ToString();
                    lObjBom.UnitCost = gvr.Cells[14].Value.ToString();
                    lObjBom.ExCost = gvr.Cells[15].Value.ToString();
                    lObjBom.UnitPrice = gvr.Cells[16].Value.ToString();
                    lObjBom.ExPrice = gvr.Cells[17].Value.ToString();

                    LstObjBom.Add(lObjBom);
                    pProjectModel.BOMs.Add(lObjBom);

                }


            }
            return LstObjBom;

        }
        private List<BOM> FillBOMModel3(ref Project pProjectModel)
        {
            List<BOM> LstObjBom = new List<BOM>();
            foreach (DataGridViewRow gvr in dataGridView3.Rows)
            {
                //string colName=gvr.Cells[0].OwningColumn.HeaderText;

                bool isAdd = false;
                for (int i = 0; i < gvr.Cells.Count; i++)
                {
                    //if (gvr.Cells[i].Value == null || gvr.Cells[i].Value == DBNull.Value || String.IsNullOrWhiteSpace(gvr.Cells[i].Value.ToString()))
                        if (gvr.Cells[i].Value == null)
                        {
                        isAdd = false;
                    }
                    else
                    {
                        isAdd = true;
                        break;
                    }
                }

                if (isAdd == true)
                {
                    BOM lObjBom = new BOM();
                    //lObjBom.BOMCode = (string)gvr.Cells[0].Value;
                    lObjBom.BOMTypeCode = 3;//(string)gvr.Cells[0].Value;
                    lObjBom.ProjectCode = pProjectModel.ProjectCode;

                    lObjBom.SORef = gvr.Cells[0].Value.ToString();
                    lObjBom.SerialNo = gvr.Cells[1].Value.ToString();
                    lObjBom.ProductCategory = gvr.Cells[2].Value.ToString();
                    lObjBom.Product = gvr.Cells[3].Value.ToString();
                    lObjBom.CostHead = gvr.Cells[4].Value.ToString();
                    lObjBom.CostSubHead = gvr.Cells[5].Value.ToString();
                    lObjBom.System = gvr.Cells[6].Value.ToString();
                    lObjBom.Area = gvr.Cells[7].Value.ToString();
                    lObjBom.Panel = gvr.Cells[8].Value.ToString();
                    lObjBom.Category = gvr.Cells[9].Value.ToString();
                    lObjBom.Manufacturer = gvr.Cells[10].Value.ToString();
                    lObjBom.PartNo = gvr.Cells[11].Value.ToString();
                    lObjBom.Description = gvr.Cells[12].Value.ToString();
                    lObjBom.Qty = gvr.Cells[13].Value.ToString();
                    lObjBom.UnitCost = gvr.Cells[14].Value.ToString();
                    lObjBom.ExCost = gvr.Cells[15].Value.ToString();
                    lObjBom.UnitPrice = gvr.Cells[16].Value.ToString();
                    lObjBom.ExPrice = gvr.Cells[17].Value.ToString();

                    LstObjBom.Add(lObjBom);
                    pProjectModel.BOMs.Add(lObjBom);

                }


            }
            return LstObjBom;

        }
        private Project FillProjectModel()
        {
            Project lObjProj = new Project();
            //if (_newMode == false) lObjProj.ProjectCode = decimal.Parse(txtProjectCode.Text);
            lObjProj.ProjectCode = decimal.Parse(txtProjectCode.Text);
            lObjProj.ProjectName = txtProjectName.Text;
            lObjProj.EndUser = txtProjectEndUser.Text;
            lObjProj.Customer = txtProjectCustomerName.Text;
            return lObjProj;

        }

        private void dataGridViewProjects_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewProjects.SelectedCells.Count > 0 && dataGridViewProjects.SelectedCells[0].Value != DBNull.Value)
            {
                int selectedrowindex = dataGridViewProjects.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridViewProjects.Rows[selectedrowindex];
                _projectCode = Convert.ToDecimal(selectedRow.Cells["ProjectCode"].Value);
                //MessageBox.Show(a);
                if ( !string.IsNullOrEmpty(txtProjectCode.Text) && _projectCode == decimal.Parse(txtProjectCode.Text)) return;

                _pc = new ProjectController();
                _currentLoadedProject = _pc.GetModelByID(_projectCode);
                if (_currentLoadedProject == null) return;
                txtProjectCode.Text = _currentLoadedProject.ProjectCode.ToString();
                txtProjectName.Text = _currentLoadedProject.ProjectName;
                txtProjectCustomerName.Text = _currentLoadedProject.Customer;
                txtProjectEndUser.Text = _currentLoadedProject.Customer;

                //var source1 = new BindingSource();
                //List<BOM> list = new List<BOM> { new MyStruct("fff", "b"), new MyStruct("c", "d") };
                if (_newMode == true)
                {
                    var a = "123";
                }
                if (_newMode == false)
                {
                    //List<BOM> allList = _bc.GetModels().Where(x => x.ProjectCode == _projectCode).ToList();
                    //List<BOM> list1 = allList.Where(x => x.BOMTypeCode == 1).ToList();
                    
                    ////TO Handle just added. so that not loading agin and again.
                    //if (_LstProjects.Where(x => x.ProjectCode == _projectCode).FirstOrDefault() == null) 
                    //{
                    //    //ClearAll();
                    //    return;
                    //}

                    List<BOM> list1 = _LstProjects.Where(x => x.ProjectCode == _projectCode).FirstOrDefault().BOMs.Where(y => y.BOMTypeCode == 1).ToList();
                    _dtSalesBOM = ToDataTable<BOM>(list1);
                    _dtSalesBOM.Columns.Remove("ProjectCode");
                    _dtSalesBOM.Columns.Remove("RowAuto");
                    _dtSalesBOM.Columns.Remove("BomTypeCode");
                    _dtSalesBOM.Columns.Remove("BOMType");
                    _dtSalesBOM.Columns.Remove("Project");
                    dataGridView1.DataSource = _dtSalesBOM;
                    //dataGridView1.DataBind();

                    //source1.DataSource = list.Where(x => x.BOMTypeCode == 1).Select(x => new
                    //{
                    //    x.SORef,
                    //    x.SerialNo,
                    //    x.ProductCategory,
                    //    x.Product,
                    //    x.CostHead,
                    //    x.CostSubHead,
                    //    x.System,
                    //    x.Area,
                    //    x.Panel,
                    //    x.Category,
                    //    x.Manufacturer,
                    //    x.PartNo,
                    //    x.Description,
                    //    x.Qty,
                    //    x.UnitCost,
                    //    x.ExCost,
                    //    x.UnitPrice,
                    //    x.ExPrice
                    //});
                    //dataGridView1.DataSource = source1;
                    //this.dataGridView1.AllowUserToAddRows = true;


                    //List<BOM> list2 = allList.Where(x => x.BOMTypeCode == 2).ToList();
                    List<BOM> list2 = _LstProjects.Where(x => x.ProjectCode == _projectCode).FirstOrDefault().BOMs.Where(y => y.BOMTypeCode == 2).ToList();
                    _dtDesignBOM = ToDataTable<BOM>(list2);
                    _dtDesignBOM.Columns.Remove("ProjectCode");
                    _dtDesignBOM.Columns.Remove("RowAuto");
                    _dtDesignBOM.Columns.Remove("BomTypeCode");
                    _dtDesignBOM.Columns.Remove("BOMType");
                    _dtDesignBOM.Columns.Remove("Project");
                    dataGridView2.DataSource = _dtDesignBOM ;

                    //List<BOM> list3 = allList.Where(x => x.BOMTypeCode == 3).ToList();
                    List<BOM> list3 = _LstProjects.Where(x => x.ProjectCode == _projectCode).FirstOrDefault().BOMs.Where(y => y.BOMTypeCode == 3).ToList();
                    _dtActualBOM = ToDataTable<BOM>(list3);
                    _dtActualBOM.Columns.Remove("ProjectCode");
                    _dtActualBOM.Columns.Remove("RowAuto");
                    _dtActualBOM.Columns.Remove("BomTypeCode");
                    _dtActualBOM.Columns.Remove("BOMType");
                    _dtActualBOM.Columns.Remove("Project");
                    dataGridView3.DataSource = _dtActualBOM;
                }
            }

        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            txtProjectCode.Text = string.Empty;
            ClearAll();
            _newMode = true;
        }

         private void ClearAll()
        {
            if (_dtSalesBOM != null) _dtSalesBOM.Rows.Clear();
            if (_dtDesignBOM != null) _dtDesignBOM.Rows.Clear();
            if (_dtActualBOM != null) _dtActualBOM.Rows.Clear();
            
            //preReq
            
            //int maxId = db.Customers.DefaultIfEmpty().Max(p => p == null ? 0 : p.Id);
            decimal maxCode= _pc.GetMaxProjectCode();
            txtProjectCode.Text = maxCode.ToString();

            txtProjectName.Text = "New Project " + maxCode;
            txtProjectCustomerName.Text = "New Customer " + maxCode;
            txtProjectEndUser.Text = "New EndUser " + maxCode;
            txtBOMFilePath.Text = string.Empty;
            //_pc.GetModels
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            if (splitContainer1.SplitterDistance < 51)
            {
                splitContainer1.SplitterDistance = 400;
            }
            else
            {
                splitContainer1.SplitterDistance = 49;
            }
            

        }

        //private void dataGridView2_MouseClick(object sender, MouseEventArgs e)
        //{
        //    //mnuCopyAllToDesignBOM.ShowDropDown();
        //    //contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
        //    if (e.Button == System.Windows.Forms.MouseButtons.Right)
        //    {
        //        MenuStripDesignBOM.Show(Cursor.Position);
        //    }
        //}
        //private void mnuCopyAllToActualBOM_Click(object sender, EventArgs e)
        //{
        //    DataTable dtActualBOM = new DataTable();
        //    dtActualBOM = _dtDesignBOM.Copy();

        //    //dataGridView2.DataSource =dataGridView1.DataSource;
        //    dataGridView3.DataSource = dtActualBOM;
        //    tabControl1.SelectedTab = tabActualBOM;

        //}
        private void dataGridViewProjects_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Add this
                dataGridViewProjects.CurrentCell = dataGridViewProjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
                // Can leave these here - doesn't hurt
                dataGridViewProjects.Rows[e.RowIndex].Selected = true;
                dataGridViewProjects.Focus();

                
            }
        }
        private void dataGridViewProjects_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                
                MenuStripProjects.Show(Cursor.Position);
            }
        }

        private void itemDeleteProject_Click(object sender, EventArgs e)
        {
            if (dataGridViewProjects.Rows.Count > 0 && dataGridViewProjects.SelectedRows.Count > 0)
            {

                // int selectedrowindex = dataGridViewProjects.SelectedCells[0].RowIndex;
                // DataGridViewRow selectedRow = dataGridViewProjects.Rows[selectedrowindex];
                // _projectCode = Convert.ToDecimal(selectedRow.Cells["ProjectCode"].Value);

                //// _dtProjects.Rows.Remove(selectedRow);
                // dataGridViewProjects.DataSource = _dtProjects;

                foreach (DataGridViewRow sr in this.dataGridViewProjects.SelectedRows)
                {

                    //Project project = (Project)item.DataBoundItem;
                    decimal pc = Convert.ToDecimal(sr.Cells[0].Value);
                    Project proj = _LstProjects.Where(x => x.ProjectCode == pc).FirstOrDefault();
                    if (proj != null) _LstProjects.Remove(proj);
                    _dtProjects.Rows.RemoveAt(sr.Index);
                    //_LstProjects.RemoveAt()
                    _pc.DeleteModel(proj.ProjectCode);

                    DataView dv = _dtProjects.DefaultView;
                    dv.Sort = "ProjectCode desc";
                    _dtProjects = dv.ToTable();
                    dataGridViewProjects.DataSource = _dtProjects;

                    if (_dtProjects.Rows.Count == 0)
                    {
                        ClearAll();
                        _pc.ReseedPk();
                        _newMode = true;
                    }


                    //dataGridViewProjects.Refresh();
                }
            }
            else
            {
                ClearAll();
            }
        }

      
    }
}
