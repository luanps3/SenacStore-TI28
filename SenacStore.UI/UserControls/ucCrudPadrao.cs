using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SenacStore.UI.Handlers;

namespace SenacStore.UI.UserControls
{
    public partial class ucCrudPadrao : UserControl, IRefreshable
    {

        private readonly ICrudHandler _handler;
        private List<object> _allData = new List<object>();
        public ucCrudPadrao(ICrudHandler handler)
        {
            InitializeComponent();
            _handler = handler ?? throw new ArgumentNullException(nameof(handler)); //atribui o handler ou lança null        }
            lblTitulo.Text = _handler.Titulo; //define o título da tela com base no handler

        }
    }
}
