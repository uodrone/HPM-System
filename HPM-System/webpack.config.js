const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const glob = require('glob');

module.exports = (env, argv) => {
    const isProduction = argv.mode === 'production';

    // Получаем все JS и CSS файлы с правильными путями
    const jsFiles = glob.sync('./wwwroot/js/**/*.js').map(file =>
        path.resolve(__dirname, file)
    );
    const cssFiles = glob.sync('./wwwroot/css/**/*.css').map(file =>
        path.resolve(__dirname, file)
    );

    // Объединяем все файлы в одну точку входа
    const allFiles = [...jsFiles, ...cssFiles];

    return {
        entry: {
            'app': allFiles
        },
        output: {
            filename: isProduction ? 'app.min.js' : 'app.js',
            path: path.resolve(__dirname, 'wwwroot/dist'),
            clean: true,
        },
        module: {
            rules: [
                {
                    test: /\.css$/i,
                    use: [
                        MiniCssExtractPlugin.loader,
                        'css-loader'
                    ],
                },
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: ['@babel/preset-env']
                        }
                    }
                }
            ],
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: isProduction ? 'app.min.css' : 'app.css',
            }),
        ],
        optimization: {
            minimize: isProduction,
            minimizer: [
                new TerserPlugin({
                    terserOptions: {
                        compress: {
                            drop_console: isProduction,
                        },
                    },
                }),
                new CssMinimizerPlugin(),
            ],
        },
        resolve: {
            extensions: ['.js', '.css'],
        },
        devtool: isProduction ? false : 'source-map',
    };
};