# Carbon Aware SDK Web API demo in Jupyter Notebook with Python SDK

This demo provides a ready-to-use [jupyter notebook](https://jupyter.org/) displaying an example thought process behind Time shifting and Location Shifting for a software engineer/data analyst.

This guide assumes the `~` directory is an alias to the directory where `carbon-aware-sdk` repository was cloned. Replace it with the appropriate path.

For help or feedback reach out via GitHub Issues or Slack.

## Prerequisites:
- Python 3.8+ (or [pyenv](https://github.com/pyenv/pyenv)) with [virtualenv package](https://pypi.org/project/virtualenv/) / [Conda](https://docs.conda.io/en/latest/miniconda.html)
- [Docker](https://docs.docker.com/get-docker/) (Desktop or CLI)
- Deployed [Carbon Aware SDK API](../../docs/quickstart.md#setting-up-the-web-api) - locally or in cloud.
- Generated Python client using [Swagger codegen](https://swagger.io/tools/swagger-codegen/) - follow [client generation guide here](../../src/clients/README.md). It is easier if you generate the client library inside of this (`python-jupyter-notebook-webapi`) directory.

## Set up guide (MacOS/Linux):

After Generating the Python client for the Carbon Aware SDK (with swagger openapi-generator), create your virtual environment to work in. It is easier to generate the client library inside of `python-jupyter-notebook-webapi` directory or move it here.

1. Update conda: `conda update conda`
2. Create the environment (Call it `sdktest`) with `conda create -n sdktest python=3.8` and you will need to select 'Y' at the prompt to proceed.
3. Activate the environment: `source activate sdktest`.
4. Install all required packages: `pip install -r requirements.txt`.
5. Get some space back: `conda clean -a`
6. Change directory into the generated client directory in order to install it. If moved to this directory: `cd python/`.
7. Install the carbon aware SDK `python setup.py install`
8. Change directory back to the sample: `cd ~/carbon-aware-sdk/samples/python-jupyter-notebook-webapi`.
9. Copy the `.env.template` file and rename it to `.env`: `cp .env.template .env`
10. Edit the `.env` file and fill in with the url of a deployed Carbon Aware SDK Web API or your own. By default the url is pointing at `https://localhost:5073`. Command: `echo "SDK_WEB_HOST = '<CARBON_AWARE_SDK_API_URL>'" > .env`
11. Start the notebook server: `jupyter-lab`
12. Open the `Carbon_aware_SDK_demo.ipynb` notebook and follow the flow of it.
12. When you're done, execute `conda deactivate` to deactivate the environment and then `conda env remove -n sdktest` to remove it.
