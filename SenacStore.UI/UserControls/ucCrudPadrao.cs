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

        public void RefreshGrid()
        {
            try
            {
                dgvDados.DataSource = null;
                var data = _handler.ObterTodos() as System.Collections.IEnumerable;
                _allData = data?.Cast<object>().ToList() ?? new List<object>();
                dgvDados.DataSource = _allData;
            }
            catch (Exception ex)
            {
                mdMessage.Show($"Erro ao carregar dados: {ex.Message}");
            }
        }

        private void dgvDados_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                // Usa a fonte ATUAL do grid (filtrada ou completa), não chama novamente o handler.
                var ds = dgvDados.DataSource as System.Collections.IEnumerable;
                var data = ds?.Cast<object>().FirstOrDefault();
                if (data == null) return; // se não há dados, sai
                var prop = data.GetType().GetProperty("FotoUrl"); // procura pela propriedade FotoUrl
                if (prop == null) return; // se não existir, nada a fazer

                // Se já existir a coluna de imagem 'Foto', remove para evitar duplicação
                if (dgvDados.Columns.Contains("Foto"))
                    dgvDados.Columns.Remove("Foto");

                // Cria e configura a coluna de imagem (DataGridViewImageColumn)
                var imgCol = new DataGridViewImageColumn
                {
                    Name = "Foto",
                    HeaderText = "Foto",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 60,
                    ReadOnly = true
                };
                dgvDados.Columns.Insert(0, imgCol); // Insere a coluna na primeira posição

                // Se existir a coluna bound 'FotoUrl', esconde-a (não mostrar texto do caminho)
                if (dgvDados.Columns.Contains("FotoUrl"))
                    dgvDados.Columns["FotoUrl"].Visible = false;

                // Determina se o grid atual é de produtos verificando presença de colunas Categoria ou Preco
                bool isProductGrid = dgvDados.Columns.Contains("Categoria") || dgvDados.Columns.Contains("Preco");

                // Itera sobre as linhas e preenche a coluna de imagem para cada linha
                for (int i = 0; i < dgvDados.Rows.Count; i++)
                {
                    try
                    {
                        var row = dgvDados.Rows[i]; // linha atual
                        var fotoVal = row.Cells["FotoUrl"]?.Value as string; // valor do caminho relativo
                        var img = LoadImageForGrid(fotoVal, isProductGrid); // carrega imagem com fallback
                        row.Cells["Foto"].Value = img; // atribui imagem à célula de imagem
                        row.Height = 60; // ajusta altura da linha para caber a imagem
                    }
                    catch
                    {
                        // Se falhar para uma linha, ignora e continua (não interrompe o carregamento)
                    }
                }

                dgvDados.Refresh(); // força repaint do grid
                dgvDados.Invalidate(); // invalida para garantir atualização visual
            }
            catch
            {
                // Silencia exceções de UI para não travar a aplicação
            }
        }

        private static Image LoadImageForGrid(string fotoUrl, bool isProduct = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fotoUrl))
                {
                    // Se não tiver foto, retorna recurso embedado: caixa para produtos, user2 para usuários
                    return isProduct ? Properties.Resources.caixa : Properties.Resources.user2;
                }

                var rel = fotoUrl.Replace('/', Path.DirectorySeparatorChar); // normaliza separador
                var fisico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rel); // caminho físico completo

                if (File.Exists(fisico))
                {
                    using var imgTemp = Image.FromFile(fisico); // carrega imagem do arquivo
                    return new Bitmap(imgTemp); // devolve cópia como Bitmap
                }

                // Se arquivo não existir, tenta fallback em disco e depois recurso embedado
                if (isProduct)
                {
                    var defaultProd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", "produtos", "default.jpg");
                    if (File.Exists(defaultProd))
                    {
                        using var imgTemp = Image.FromFile(defaultProd);
                        return new Bitmap(imgTemp);
                    }
                    return Properties.Resources.caixa; // resource padrão para produto
                }
                else
                {
                    var defaultUser = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", "user2.png");
                    if (File.Exists(defaultUser))
                    {
                        using var imgTemp = Image.FromFile(defaultUser);
                        return new Bitmap(imgTemp);
                    }
                    return Properties.Resources.user2; // resource padrão para usuário
                }
            }
            catch
            {
                // Se qualquer erro ocorrer, tenta retornar recurso apropriado; se falhar, retorna null
                try { return isProduct ? Properties.Resources.caixa : Properties.Resources.user2; } catch { return null; }
            }
        }

        private void btnNovo_Click(object sender, EventArgs e)
        {
            _handler.Criar();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvDados.SelectedRows.Count == 0) return;
            var id = (Guid)dgvDados.SelectedRows[0].Cells["Id"].Value;
            _handler.Editar(id);
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (dgvDados.SelectedRows.Count == 0) return;
            var id = (Guid)dgvDados.SelectedRows[0].Cells["Id"].Value;

            var dialog = new Guna.UI2.WinForms.Guna2MessageDialog
            {
                Text = $"Confirma a exclusão do registro selecionado? ",
                Caption = "Confirmação",
                Buttons = Guna.UI2.WinForms.MessageDialogButtons.YesNo,
                Icon = Guna.UI2.WinForms.MessageDialogIcon.Question,
                Style = Guna.UI2.WinForms.MessageDialogStyle.Default
            };

            var resposta = dialog.Show();

            if (resposta == DialogResult.Yes)
            {
                _handler.Deletar(id);
                RefreshGrid();
            }

        }

        private void btnAtualizar_Click(object sender, EventArgs e) => RefreshGrid();

    }
}
