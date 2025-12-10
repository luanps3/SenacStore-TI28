using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SenacStore.Domain.Entities;
using SenacStore.UI.Navigation;

namespace SenacStore.UI.UserControls
{
    public partial class ucTipoUsuario : UserControl
    {
        //Dependências Injetadas
        private readonly ICrudNavigator _nav; // navigator para abrir UserControls no container principal
        private readonly ITipoUsuarioRepository _repo; // repositório que realiza operações no banco
        private readonly Guid? _id; // id do TipoUsuario a ser editado (null para criação)

        public ucTipoUsuario(ICrudNavigator nav, ITipoUsuarioRepository repo, Guid? id = null)
        {
            InitializeComponent();
            _nav = nav;
            _repo = repo;
            _id = id;

            if (_id.HasValue) CarregarTipo(_id.Value);
        }

        private void CarregarTipo(Guid id)
        {
            var tipo = _repo.ObterPorId(id);
            if (tipo == null) return;
            txtNome.Text = tipo.Nome;
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome obrigatório", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (_id.HasValue)
            {
                //Modo edição
                var tipo = _repo.ObterPorId(_id.Value);
                tipo.Nome = txtNome.Text.Trim();
                _repo.Atualizar(tipo);
            }
            else
            {
                //Modo criação
                var novoTipo = new TipoUsuario
                {
                    Id = Guid.NewGuid(),
                    Nome = txtNome.Text.Trim()
                };
                _repo.Criar(novoTipo);
            }
            _nav.Voltar();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _nav.Voltar();
        }
    }
}
