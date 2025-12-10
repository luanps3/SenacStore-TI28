using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SenacStore.Domain.Entities;
using SenacStore.UI.Navigation;

namespace SenacStore.UI.UserControls
{
    public partial class ucProdutos : UserControl
    {
        private readonly ICrudNavigator _nav;
        private readonly IProdutoRepository _produtoRepo;
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly Guid? _id;

        private byte[] _fotoTempBytes;
        private string _fotoRelativa;

        public ucProdutos(
            ICrudNavigator nav,
            IProdutoRepository produtoRepo,
            ICategoriaRepository categoriaRepo,
            Guid? id = null)
        {
            InitializeComponent();
            _nav = nav;
            _produtoRepo = produtoRepo;
            _categoriaRepo = categoriaRepo;
            _id = id;

            CarregarCategorias();

            if (_id.HasValue)
            {
                CarregarProduto(_id.Value);
            }
        }

        private void CarregarCategorias()
        {
            var categorias = _categoriaRepo.ObterTodos();
            cboCategoria.DataSource = categorias;
            cboCategoria.DisplayMember = "Nome";
            cboCategoria.ValueMember = "Id";
        }

        private void CarregarProduto(Guid id)
        {
            var p = _produtoRepo.ObterPorId(id);      // Busca produto no repositório
            if (p == null) return;                    // Se não encontrado, sai

            txtNome.Text = p.Nome;                    // Preenche nome
            txtPreco.Text = p.Preco.ToString("N2", CultureInfo.CurrentCulture); // Formata preço conforme cultura
            cboCategoria.SelectedValue = p.CategoriaId; // Seleciona categoria correspondente

            // Se o produto já tem FotoUrl, tenta carregar a imagem no PictureBox
            if (!string.IsNullOrWhiteSpace(p.FotoUrl))
            {
                try
                {
                    // Converte caminho relativo salvo no DB para caminho físico
                    var caminhoFisico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p.FotoUrl.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(caminhoFisico))
                    {
                        using var imgTemp = Image.FromFile(caminhoFisico); // Abre arquivo de imagem
                        pbFoto.Image = new Bitmap(imgTemp);               // Atribui cópia Bitmap ao PictureBox
                        _fotoRelativa = p.FotoUrl;                        // Guarda caminho relativo existente
                    }
                }
                catch
                {
                    // Falha ao carregar imagem: ignoramos para não impedir edição do produto
                }
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            // Validações básicas do formulário
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                mdMessage.Show("Nome do produto é obrigatório.", "Atenção");
                txtNome.Focus();
                return;
            }

            if (cboCategoria.SelectedValue == null)
            {
                mdMessage.Show("Selecione uma categoria.", "Atenção");
                cboCategoria.Focus();
                return;
            }

            // Parse seguro do preço usando a cultura corrente (aceita vírgula/ponto conforme sistema)
            if (!decimal.TryParse(txtPreco.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var preco))
            {
                mdMessage.Show("Preço inválido. Informe um número válido (ex.: 429,90).", "Atenção");
                txtPreco.Focus();
                return;
            }

            try
            {
                if (_id.HasValue)
                {
                    // Edição: busca entidade, atualiza campos e salva
                    var p = _produtoRepo.ObterPorId(_id.Value);
                    if (p == null)
                    {
                        mdMessage.Show("Produto não encontrado.", "Erro");
                        return;
                    }

                    p.Nome = txtNome.Text.Trim();                 // Atualiza nome
                    p.Preco = preco;                              // Atualiza preço
                    p.CategoriaId = (Guid)cboCategoria.SelectedValue; // Atualiza categoria

                    if (_fotoTempBytes != null)                   // Se houve upload de nova imagem
                    {
                        // Salva imagem física em "img/produtos/{produtoId}.jpg" para unicidade
                        var pastaRel = Path.Combine("img", "produtos");
                        var pastaFisica = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pastaRel);
                        if (!Directory.Exists(pastaFisica)) Directory.CreateDirectory(pastaFisica);

                        var nomeArquivo = $"{p.Id}.jpg";           // Nome único baseado no Id do produto
                        var destino = Path.Combine(pastaFisica, nomeArquivo);
                        File.WriteAllBytes(destino, _fotoTempBytes); // Grava bytes no arquivo
                        p.FotoUrl = Path.Combine(pastaRel, nomeArquivo).Replace('\\', '/'); // Seta caminho relativo
                    }

                    _produtoRepo.Atualizar(p);                     // Persiste alterações
                }
                else
                {
                    // Criação: monta nova entidade e persiste
                    var novo = new Produto
                    {
                        Id = Guid.NewGuid(),
                        Nome = txtNome.Text.Trim(),
                        Preco = preco,
                        CategoriaId = (Guid)cboCategoria.SelectedValue
                    };

                    if (_fotoTempBytes != null) // Se carregou imagem antes de criar
                    {
                        var pastaRel = Path.Combine("img", "produtos");
                        var pastaFisica = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pastaRel);
                        if (!Directory.Exists(pastaFisica)) Directory.CreateDirectory(pastaFisica);

                        var nomeArquivo = $"{novo.Id}.jpg";
                        var destino = Path.Combine(pastaFisica, nomeArquivo);
                        File.WriteAllBytes(destino, _fotoTempBytes); // Salva arquivo físico
                        novo.FotoUrl = Path.Combine(pastaRel, nomeArquivo).Replace('\\', '/'); // Define FotoUrl
                    }

                    _produtoRepo.Criar(novo); // Persiste novo produto no repositório
                }

                _nav.Voltar(); // Após salvar, volta para a lista (ou controle anterior)
            }
            catch (Exception ex)
            {
                // Em caso de erro durante persistência, informa usuário (útil para diagnóstico)
                mdMessage.Show($"Erro ao salvar produto: {ex.Message}", "Erro");
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _nav.Voltar();
        }

        private void txtPreco_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtPreco.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var valor))
            {
                txtPreco.Text = valor.ToString("N2", CultureInfo.CurrentCulture);
            }
        }

        private void txtPreco_KeyPress(object sender, KeyPressEventArgs e)
        {
            var decimalSep = Convert.ToChar(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator); // Separador decimal da cultura
            var allowed = char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar) || e.KeyChar == decimalSep; // Permissões
            if (!allowed) e.Handled = true;                      // Bloqueia teclas inválidas

            // Se já existe separador decimal no texto, bloqueia entrada de outro
            var tb = sender as TextBox;
            if (e.KeyChar == decimalSep && tb != null && tb.Text.Contains(decimalSep))
            {
                e.Handled = true;
            }
        }

        private void pbFoto_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();                  // Cria diálogo de seleção de arquivo
            ofd.Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp";       // Filtra tipos suportados
            ofd.Title = "Selecione a imagem do produto";
            if (ofd.ShowDialog() != DialogResult.OK) return;       // Se cancelar, termina

            try
            {
                _fotoTempBytes = File.ReadAllBytes(ofd.FileName);  // Lê todos os bytes do ficheiro selecionado
                using var ms = new MemoryStream(_fotoTempBytes);   // Cria memória stream para construir imagem
                pbFoto.Image = Image.FromStream(ms);               // Mostra a imagem no PictureBox
            }
            catch (Exception ex)
            {
                // Em caso de falha ao ler/carregar imagem, informa o usuário
                mdMessage.Show($"Erro ao carregar imagem: {ex.Message}", "Erro");
            }
        }

        private void ucProdutos_Load(object sender, EventArgs e)
        {

        }
    }
}
